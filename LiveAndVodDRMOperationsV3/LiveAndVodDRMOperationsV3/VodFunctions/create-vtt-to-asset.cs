//
// Azure Media Services REST API v3 Functions
//
// create-vtt-to-asset
/*
This function start the copy of blobs to an asset container.


```c#
Input:
{
    "assetName" : "asset-dgdccfcffs", // optional. Will be automatically generated if not provided
    "vttFileName" : "english.vtt",
    "vttContent" : ,
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, the live event will be deleted from the two regions.
            // Note: the service principal must work with all this accounts
}

Output:
{
    "success": true,
    "errorMessage" : "",
    "operationsVersion": "1.0.0.5",
    "missingBlob" : "True" // True if one of the source blob(s) is missing
    "container" : [
    {"the name of the storage container of the asset"} 
    ]

}

```
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LiveDrmOperationsV3
{
    public static class createVttToAsset
    {
        [FunctionName("create-vtt-to-asset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext execContext)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var assetName = (string)data.assetName;
            if (assetName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass assetName in the JSON");

            var vttContent = (string)data.vttContent;
            if (vttContent == null)
                return IrdetoHelpers.ReturnErrorException(log, "Please pass vttContent in the input object");

            var vttFileName = (string)data.vttFileName;
            if (vttFileName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Please pass vttFileName in the input object");


            // Azure region management
            var azureRegions = new List<string>();
            if ((string)data.azureRegion != null)
            {
                azureRegions = ((string)data.azureRegion).Split(',').ToList();
            }
            else
            {
                azureRegions.Add((string)null);
            }

            foreach (var region in azureRegions)
            {
                ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .Build(),
                        region
                );

                MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                // Set the polling interval for long running operations to 2 seconds.
                // The default value is 30 seconds for the .NET client SDK
                client.LongRunningOperationRetryTimeout = 2;

                MediaServicesHelpers.LogInformation(log, "asset name : " + assetName, region);

                try
                {
                    var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, assetName);

                    // Access to container
                    ListContainerSasInput input = new ListContainerSasInput()
                    {
                        Permissions = AssetContainerPermission.ReadWrite,
                        ExpiryTime = DateTime.Now.AddHours(6).ToUniversalTime()
                    };

                    var responseListSas = await client.Assets.ListContainerSasAsync(config.ResourceGroup, config.AccountName, asset.Name, input.Permissions, input.ExpiryTime);
                    string uploadSasUrl = responseListSas.AssetContainerSasUrls.First();
                    var sasUri = new Uri(uploadSasUrl);
                    var destinationBlobContainer = new CloudBlobContainer(sasUri);
                    var destinationBlob = destinationBlobContainer.GetBlockBlobReference(vttFileName);
                    await destinationBlob.UploadTextAsync(vttContent);
                }
                catch (Exception ex)
                {
                    return IrdetoHelpers.ReturnErrorException(log, ex);
                }
            }

            var response = new JObject
            {
                {"success", true},
                {"assetName",  assetName},
                {
                    "operationsVersion",
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                }
            };

            return new OkObjectResult(
                response
            );
        }
    }
}