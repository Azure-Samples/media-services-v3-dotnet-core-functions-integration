//
// Azure Media Services REST API v3 Functions
//
// DeleteContentKeyPolicy - This function deletes an ContentKeyPolicy object.
//
/*
```c#
Input:
    {
        // [Required] The content key policy name.
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
    }
Output:
    {
        // The name of the deleted content key policy.
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
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
    public static class DeleteContentKeyPolicy
    {
        [FunctionName("DeleteContentKeyPolicy")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - CreateContentKeyPolicy was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.contentKeyPolicyName == null)
                return new BadRequestObjectResult("Please pass contentKeyPolicyName in the input object");
            string contentKeyPolicyName = data.contentKeyPolicyName;

            try
            {
                MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                client.ContentKeyPolicies.Delete(amsconfig.ResourceGroup, amsconfig.AccountName, contentKeyPolicyName);
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
                contentKeyPolicyName = contentKeyPolicyName
            });
        }
    }
}
