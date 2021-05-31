//
// Azure Media Services REST API v3 Functions
//
// CreateTransform - This function creates a new transform.
//
/*
```c#
Input:
    {
        // [Required] The name of the transform.
        "transformName": "TestTransform"
    }
Output:
    {
        // The name of the created TransformName
        "transformName": "TestTransform",

        // The resource identifier of the created Transform
        "transformId": "/subscriptions/694d5930-8ee4-4e50-917b-9dcfeceb6179/resourceGroups/AMSdemo/providers/Microsoft.Media/mediaservices/amsdemojapaneast/transforms/TestTransform"
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
    public static class GetTransform
    {
        [FunctionName("GetTransform")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - GetTransform was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.transformName == null)
                return new BadRequestObjectResult("Please pass transformName in the input object");
            string transformName = data.transformName;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string transformId = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                Transform transform = client.Transforms.Get(amsconfig.ResourceGroup, amsconfig.AccountName, transformName);
                if (transform == null)
                    transformName = null;
                else
                    transformId = transform.Id;
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

            return (ActionResult)new OkObjectResult(new
            {
                transformName = transformName,
                transformId = transformId
            });
        }
    }
}
