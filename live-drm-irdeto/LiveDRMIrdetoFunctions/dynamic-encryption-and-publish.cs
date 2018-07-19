/*
Input:

{
"assetId":"",
"streamingPolicyName":"",
"streamingEndpointName": "default, // optional"
"streamingLocatorId": "" // optional, to force a specific Guid

// optional, to not use Irdeto back-end to get the keys:
"cencKeyId":"",
"cbcsKeyId":"",
"contentCencKey":"",
"contentCbcsKey":"",
}
*/


using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;


namespace ImgDrmOperationsV2
{
    public static class DynEncryption
    {
        private const string labelCenc = "cencDefaultKey";
        private const string labelCbcs = "cbcsDefaultKey";
        private const string assetprefix = "nb:cid:UUID:";

        [FunctionName("dynamic-encryption-and-publish")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            string assetId = (string)data.assetId;
            if (assetId == null)
                return new BadRequestObjectResult("Error - please pass assetId in the JSON");

            string streamingPolicyName = (string)data.streamingPolicyName;
            if (streamingPolicyName == null)
                return new BadRequestObjectResult("Error - please pass streamingPolicyName in the JSON");

            string contentId = streamingPolicyName.Substring(0, streamingPolicyName.IndexOf("-"));

            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddEnvironmentVariables()
             .Build());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error - " + ex.Message);
            }

            log.Info("config loaded.");

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            string cenckeyId = null;
            string cbcskeyId = null;

            try
            {
                // let's get the key if not in manual mode
                cenckeyId = data.cencKeyId;
                string cenccontentKey = data.contentCencKey;
                cbcskeyId = data.cbcsKeyId;
                string cbcscontentKey = data.contentCbcsKey;

                if (cenckeyId == null || cbcskeyId == null)
                {
                    log.Info("Irdeto call...");
                    var responsecenc = await IrdetoHelpers.CreateSoapEnvelopGenerateKeys(config.IrdetoSoapService, contentId, config, false);

                    cenckeyId = IrdetoHelpers.ReturnDataFromSoapResponse(responsecenc, "KeyId=");
                    cenccontentKey = IrdetoHelpers.ReturnDataFromSoapResponse(responsecenc, "ContentKey=");

                    var responsecbcs = await IrdetoHelpers.CreateSoapEnvelopGenerateKeys(config.IrdetoSoapService, contentId, config, true);

                    cbcskeyId = IrdetoHelpers.ReturnDataFromSoapResponse(responsecbcs, "KeyId=");
                    cbcscontentKey = IrdetoHelpers.ReturnDataFromSoapResponse(responsecbcs, "ContentKey=");

                    if (cenckeyId == null || cbcskeyId == null)
                        return new BadRequestObjectResult("Error - Irdeto response does not contain keys");
                }

                // Creating a unique suffix so that we don't have name collisions if you run the sample
                // multiple times without cleaning up.
                string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);

                string streamingLocatorName = "locator-" + uniqueness;

                // let's get the asset
                // in v3, asset name = asset if in v2 (without prefix)
                log.Info("Asset configuration.");
                assetId = assetId.StartsWith(assetprefix) ? assetId.Substring(assetprefix.Length) : assetId;
                Asset asset = client.Assets.Get(config.ResourceGroup, config.AccountName, assetId);

                StreamingLocatorUserDefinedContentKey keyCenc = new StreamingLocatorUserDefinedContentKey()
                {
                    Id = Guid.Parse(cenckeyId),
                    Label = labelCenc + contentId,
                    Value = cenccontentKey
                };
                StreamingLocatorUserDefinedContentKey keyCbcs = new StreamingLocatorUserDefinedContentKey()
                {
                    Id = Guid.Parse(cbcskeyId),
                    Label = labelCbcs + contentId,
                    Value = cbcscontentKey
                };

                
                StreamingLocator locator = new StreamingLocator(
                      assetName: asset.Name,
                      streamingPolicyName: (string)data.streamingPolicyName,
                      defaultContentKeyPolicyName:  null,
                      contentKeys: new List<StreamingLocatorUserDefinedContentKey>() { keyCenc, keyCbcs },
                     /* type: StreamingLocatorContentKeyType.CommonEncryptionCenc, */  // removed July 2nd
                      streamingLocatorId: (data.streamingLocatorId != null) ? (Guid?)Guid.Parse((string)data.streamingLocatorId) : null
                      );

                var sl = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, streamingLocatorName, locator);

                var streamingEndpoint = client.StreamingEndpoints.Get(config.ResourceGroup, config.AccountName, (string)data.streamingEndpointName ?? "default");

                // Get the URls to stream the output
                var paths = client.StreamingLocators.ListPaths(config.ResourceGroup, config.AccountName, streamingLocatorName);

                log.Info("The urls to stream the output from a client:");

                List<string> urls = new List<string>();

                for (int i = 0; i < paths.StreamingPaths.Count; i++)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "https";
                    uriBuilder.Host = streamingEndpoint.HostName;

                    if (paths.StreamingPaths[i].Paths.Count > 0)
                    {
                        uriBuilder.Path = paths.StreamingPaths[i].Paths[0];
                        var myPath = uriBuilder.ToString();
                        if (myPath.Contains("(format=mpd-time-csf,encryption=cenc)")) // lets expose only dans cenc for this project
                            urls.Add(myPath);
                    }
                }

                return (ActionResult)new OkObjectResult($"Streaming locator created : {sl.Name}" + Environment.NewLine + $"cencKeyId : {cenckeyId}" + Environment.NewLine + $"cbcsKeyId : {cbcskeyId}" + Environment.NewLine + "URls : " + Environment.NewLine + String.Join(Environment.NewLine, urls.ToArray()));
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error - " + ex.Message);
            }
        }
     
    }
}
