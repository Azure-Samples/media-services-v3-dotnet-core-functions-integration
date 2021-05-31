//
// Azure Media Services REST API v3 Functions
//
// DeleteAsset - This function deletes an existing asset.
//
/*
```c#
Input:
    {
        // [Required] The name of the asset
        "assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",

        // [Required] The name of the Azure Media Service account
        "accountName": "amsaccount",

        // [Required] The resource group of the Azure Media Service account
        "resourceGroup": "mediaservices-rg"
    }
Output:
    {
        // The name of the deleted asset
        "assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85"
    }

```
*/
//
//

using advanced_vod_functions_v3.SharedLibs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;


namespace advanced_vod_functions_v3
{
    public static class DeleteAsset
    {
        [FunctionName("DeleteAsset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - DeleteAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string assetName = data.assetName;
            string accountName = data.accountName;
            string resourceGroup = data.resourceGroup;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                client.Assets.Delete(resourceGroup, accountName, assetName);
            }
            catch (ApiErrorException e)
            {
                log.LogError($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.LogError($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            // compatible with AMS V2 API
            return (ActionResult)new OkObjectResult(new
            {
                assetName = assetName
            });
        }
    }
}
