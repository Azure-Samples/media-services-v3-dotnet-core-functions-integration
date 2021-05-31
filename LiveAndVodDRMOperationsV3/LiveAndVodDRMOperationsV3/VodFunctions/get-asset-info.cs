//
// Azure Media Services REST API v3 Functions
//
// get-asset-info
/*
This function gets asset info.


```c#
Input:
{
    "assetName" : "600febae3e-input-ebf95-ContentAwareEncode-output-ebf95"
}


Output:
{
    "success": true,
    "assetName": "600febae3e-input-ebf95-ContentAwareEncode-output-ebf95",
    "assetId": "/subscriptions/ee0da9ed-9001-4525-915e-a09f0aa94e65/resourceGroups/DimensionInsightResourceGroup/providers/Microsoft.Media/mediaservices/covid19ams/assets/600febae3e-input-ebf95-ContentAwareEncode-output-ebf95",
    "fileNames": [
        "07 Communicatio.ism",
        "07 Communicatio.ismc",
        "07 Communicatio_1280x720_AACAudio_1358.mp4",
        "07 Communicatio_1280x720_AACAudio_1358_1.mpi",
        "07 Communicatio_1920x1080_AACAudio_2757.mp4",
        "07 Communicatio_1920x1080_AACAudio_2757_1.mpi",
        "07 Communicatio_480x270_AACAudio_284.mp4",
        "07 Communicatio_480x270_AACAudio_284_1.mpi",
        "07 Communicatio_480x270_AACAudio_284_2.mpi",
        "07 Communicatio_640x360_AACAudio_452.mp4",
        "07 Communicatio_640x360_AACAudio_452_1.mpi",
        "07 Communicatio_960x540_AACAudio_856.mp4",
        "07 Communicatio_960x540_AACAudio_856_1.mpi",
        "07 Communicatio_Cae_manifest.json",
        "Thumbnail000001.jpg",
        "ebaeaa51-46e2-471b-b05f-97be5e6488de_metadata.json"
    ],
    "container": "asset-32606e0e-dac6-4404-88b4-e97cc3475591",
    "storageAccountName": "ams45amsstorage",
    "operationsVersion": "1.0.1.0"
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
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LiveDrmOperationsV3
{
    public static class GetAssetInfot
    {
        [FunctionName("get-asset-info")]
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

            List<string> fileNames = new List<string>();

            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .Build(),
                    null
            );

            MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, null);

            var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            MediaServicesHelpers.LogInformation(log, "asset name : " + assetName, null);

            Asset asset = new Asset();

            try
            {
                asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName, assetName);

                ListContainerSasInput input = new ListContainerSasInput()
                {
                    Permissions = AssetContainerPermission.Read,
                    ExpiryTime = DateTime.Now.AddHours(2).ToUniversalTime()
                };

                var responseAssetContSas = await client.Assets.ListContainerSasAsync(config.ResourceGroup, config.AccountName, assetName, input.Permissions, input.ExpiryTime);
                string uploadSasUrl = responseAssetContSas.AssetContainerSasUrls.First();
                Uri sasUri = new Uri(uploadSasUrl);
                var container = new CloudBlobContainer(sasUri);


                BlobContinuationToken continuationToken = null;
                var blobs = new List<IListBlobItem>();

                do
                {
                    BlobResultSegment segment = await container.ListBlobsSegmentedAsync(null, false, BlobListingDetails.Metadata, null, continuationToken, null, null);
                    blobs.AddRange(segment.Results);

                    foreach (IListBlobItem blob in segment.Results)
                    {
                        if (blob.GetType() == typeof(CloudBlockBlob))
                        {
                            CloudBlockBlob bl = (CloudBlockBlob)blob;
                            fileNames.Add(bl.Name);
                        }
                    }
                    continuationToken = segment.ContinuationToken;
                }
                while (continuationToken != null);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject
            {
                {"success", true},
                {"assetName",  assetName},
                {"assetId",  asset.AssetId},
                {"fileNames", new JArray(fileNames)},
                {"container", asset.Container},
                {"storageAccountName", asset.StorageAccountName},
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