//
// Azure Media Services REST API v3 Functions
//
// UnpublishAsset - This function unpublishes an asset by deleting its streaming locator and policy.
//
/*
```c#
Input:
    {
        // [Required] The name of the streaming locator
        "streamingLocatorName": "streaminglocator-911b65de-ac92-4391-9aab-80021126d403"
    }
Output:
    {
        // The name of the deleted StreamingLocatorName
        "streamingLocatorName": "streaminglocator-911b65de-ac92-4391-9aab-80021126d403"

        // The name of the deleted streamingPolicyName
        "streamingPolicyName": "SharedStreamingForClearKey"
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class UnpublishAsset
    {
        [FunctionName("UnpublishAsset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - PublishAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.streamingLocatorName == null)
                return new BadRequestObjectResult("Please pass streamingLocatorName in the input object");
            string streamingLocatorName = data.streamingLocatorName;
            string streamingPolicyName = null;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string contentKeysJson = string.Empty;
            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                var streamingLocator = client.StreamingLocators.Get(amsconfig.ResourceGroup, amsconfig.AccountName, streamingLocatorName);
                streamingPolicyName = streamingLocator.StreamingPolicyName;

                client.StreamingLocators.Delete(amsconfig.ResourceGroup, amsconfig.AccountName, streamingLocatorName);
                if (!streamingPolicyName.StartsWith("Predefined_"))
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
                streamingLocatorName = streamingLocatorName,
                streamingPolicyName = streamingPolicyName == null || streamingPolicyName.StartsWith("Predefined_") ? "None" : streamingPolicyName,
                contentKeys = contentKeysJson
            });
        }
    }
}
