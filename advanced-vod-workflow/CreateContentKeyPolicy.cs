//
// Azure Media Services REST API v3 Functions
//
// CreateContentKeyPolicy - This function creates an ContentKeyPolicy object.
//
/*
```c#
Input:
    {
        // Name of the  Content Key Policy object
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
        // (Optional) Description of the Content Key Policy object
        "contentKeyPolicyDescription": "Shared toekn restricted policy for Clear Key content key policy",
        // Options for the Content Key Policy object
        "contentKeyPolicyOptions": [
            {
                "name": "ClearKeyOption",
                "configuration": {
                    "@odata.type": "#Microsoft.Media.ContentKeyPolicyClearKeyConfiguration"
                },
                "restriction": {
                    "@odata.type": "#Microsoft.Media.ContentKeyPolicyTokenRestriction",
                    "issuer": "urn:issuer",
                    "audience": "urn:audience",
                    "primaryVerificationKey": {
                        "@odata.type": "#Microsoft.Media.ContentKeyPolicySymmetricTokenKey",
                        "keyValue": "AAAAAAAAAAAAAAAAAAAAAA=="
                    },
                    "restrictionTokenType": "Swt"
                }
            }
        ]
    }
Output:
    {
        // Id of the Content Key Policy object
        "policyId": "9d6a2b92-d61a-4e87-8348-7155c137f9ca",
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class CreateContentKeyPolicy
    {
        [FunctionName("CreateContentKeyPolicy")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - CreateEmptyAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.contentKeyPolicyName == null)
                return new BadRequestObjectResult("Please pass contentKeyPolicyName in the input object");
            if (data.contentKeyPolicyOptions == null)
                return new BadRequestObjectResult("Please pass contentKeyPolicyOptions in the input object");
            string contentKeyPolicyName = data.contentKeyPolicyName;
            string contentKeyPolicyDescription = null;
            if (data.contentKeyPolicyDescription == null)
                contentKeyPolicyDescription = data.contentKeyPolicyDescription;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            ContentKeyPolicy policy = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                JsonConverter[] jsonConverters = {
                    new MediaServicesHelperJsonConverter(),
                    new MediaServicesHelperTimeSpanJsonConverter()
                };
                List<ContentKeyPolicyOption> options = JsonConvert.DeserializeObject<List<ContentKeyPolicyOption>>(data.contentKeyPolicyOptions.ToString(), jsonConverters);

                policy = client.ContentKeyPolicies.CreateOrUpdate(amsconfig.ResourceGroup, amsconfig.AccountName, contentKeyPolicyName, options, contentKeyPolicyDescription);
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

            return (ActionResult)new OkObjectResult(new
            {
                policyId = policy.PolicyId
            });
        }
    }
}
