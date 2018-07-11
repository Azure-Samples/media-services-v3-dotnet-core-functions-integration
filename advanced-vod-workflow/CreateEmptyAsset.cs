//
// Azure Media Services REST API v3 Functions
//
// CreateEmptyAsset - This function creates an empty asset.
//
/*
```c#
Input:
    {
        // Name of the asset
        "assetNamePrefix": "TestAssetName",
        // (Optional) Name of attached storage account where to create the asset
        "assetStorageAccount":  "storage01"
    }
Output:
    {
        // Name of the asset created
        "assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",
        // Id of the asset created
        "assetId": "nb:cid:UUID:68adb036-43b7-45e6-81bd-8cf32013c810",
        // Name of the destination container name for the asset created
        "destinationContainer": "destinationContainer": "asset-4a5f429c-686c-4f6f-ae86-4078a4e6139e"
    }

```
*/
//
//

using System;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class CreateEmptyAsset
    {
        [FunctionName("CreateEmptyAsset")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - CreateEmptyAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.assetNamePrefix == null)
                return new BadRequestObjectResult("Please pass assetNamePrefix in the input object" );
            string assetStorageAccount = null;
            if (data.assetStorageAccount != null)
                assetStorageAccount = data.assetStorageAccount;
            Guid assetGuid = Guid.NewGuid();
            string assetName = data.assetNamePrefix + "-" + assetGuid.ToString();

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            Asset asset = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                Asset assetParams = new Asset(null, assetName, null, assetGuid, DateTime.Now, DateTime.Now, null, assetName, null, assetStorageAccount, AssetStorageEncryptionFormat.None);
                asset = client.Assets.CreateOrUpdate(amsconfig.ResourceGroup, amsconfig.AccountName, assetName, assetParams);
                //asset = client.Assets.CreateOrUpdate(amsconfig.ResourceGroup, amsconfig.AccountName, assetName, new Asset());
            }
            catch (ApiErrorException e)
            {
                log.Info($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.Info($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            // compatible with AMS V2 API
            string assetId = "nb:cid:UUID:" + asset.AssetId;
            string destinationContainer = "asset-" + asset.AssetId;

            return (ActionResult)new OkObjectResult(new
            {
                assetName = assetName,
                assetId = assetId,
                destinationContainer = destinationContainer
            });
        }
    }
}
