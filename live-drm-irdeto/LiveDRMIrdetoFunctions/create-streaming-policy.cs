/*
Input :

{ 
"contentId" : "SFPOC"
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
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;

namespace ImgDrmOperationsV2
{
    public static class CreateStreamingPolicy
    {
        private const string labelCenc = "cencDefaultKey";
        private const string labelCbcs = "cbcsDefaultKey";


        [FunctionName("create-streaming-policy")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string contentId = (string)data.contentId;
            if (contentId == null)
                return new BadRequestObjectResult("Error - please pass contentId in the JSON");

            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddEnvironmentVariables()
               .Build());

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                // Creating a unique suffix so that we don't have name collisions if you run the sample
                // multiple times without cleaning up.
                string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);

                log.Info("Creating streaming policy.");
                var dash_protocol = new EnabledProtocols(false, true, false, false);
                var hls_dash_protocol = new EnabledProtocols(false, true, true, false);
                var cenc_config = new CencDrmConfiguration(
                    new StreamingPolicyPlayReadyConfiguration()
                    {
                        CustomLicenseAcquisitionUrlTemplate = string.Format(config.IrdetoPlayReadyLAURL, contentId)
                    },
                    new StreamingPolicyWidevineConfiguration()
                    {
                        CustomLicenseAcquisitionUrlTemplate = string.Format(config.IrdetoWidevineLAURL, contentId)
                    }
                    );
                var cbcs_config = new CbcsDrmConfiguration(
                    new StreamingPolicyFairPlayConfiguration()
                    {
                        CustomLicenseAcquisitionUrlTemplate = string.Format(config.IrdetoFairPlayLAURL, contentId)
                    }
                    );

                var ContentKeysEnc = new StreamingPolicyContentKeys()
                {
                    DefaultKey = new DefaultKey()
                    {
                        Label = labelCenc + contentId
                    }
                };

                var ContentKeysCbcsc = new StreamingPolicyContentKeys()
                {
                    DefaultKey = new DefaultKey()
                    {
                        Label = labelCbcs + contentId
                    }
                };

                var cenc = new CommonEncryptionCenc(enabledProtocols: dash_protocol, clearTracks: null, contentKeys: ContentKeysEnc, drm: cenc_config);
                var cbcs = new CommonEncryptionCbcs(enabledProtocols: hls_dash_protocol, clearTracks: null, contentKeys: ContentKeysCbcsc, drm: cbcs_config);

                string policyname = contentId + "-" + uniqueness;
                var sp = new StreamingPolicy(Guid.NewGuid().ToString(), policyname, null, DateTime.Now, null, null, cenc, cbcs, null);
                var sp2 = await client.StreamingPolicies.CreateAsync(config.ResourceGroup, config.AccountName, policyname, sp);

                return (ActionResult)new OkObjectResult($"Streaming Policy Name Created : " + sp2.Name);

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error - " + ex.Message);
            }
        }
    }
}
