//
// Azure Media Services REST API v3 Functions
//
// DeleteStreamingPolicy - This function deletes an StreamingPolicy object.
//
/*
```c#
Input:
    {
        // [Required] The name of the streaming policy.
        "streamingPolicyName": "SharedStreamingForClearKey",
    }
Output:
    {
        // The name of the streaming policy.
        "streamingPolicyName": "SharedStreamingForClearKey",
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
    public static class DeleteStreamingPolicy
    {
        [FunctionName("DeleteStreamingPolicy")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - CreateStreamingPolicy was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.streamingPolicyName == null)
                return new BadRequestObjectResult("Please pass streamingPolicyName in the input object");
            string streamingPolicyName = data.streamingPolicyName;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                client.StreamingPolicies.Delete(amsconfig.ResourceGroup, amsconfig.AccountName, streamingPolicyName);
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
                streamingPolicyName = streamingPolicyName
            });
        }
    }
}