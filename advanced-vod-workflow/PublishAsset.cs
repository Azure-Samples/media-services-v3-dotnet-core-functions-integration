//
// Azure Media Services REST API v3 Functions
//
// PublishAsset - This function publishes the asset (creates a StreamingLocator for the asset).
//
/*
```c#
Input:
    {
        // Name of the asset for publish
        "assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",
        // Name of Streaming Policy; predefined streaming policy or custom created streaming policy
        "streamingPolicyName": "Predefined_ClearStreamingOnly",
        // (Optional) Start DateTime of streaming the asset
        "startDateTime": "2018-07-01T00:00Z",
        // (Optional) End DateTime of streaming the asset
        "endDateTime": "2018-12-31T23:59Z",
        // (Optional) Id (UUID string) of the StreamingLocator; streamingLocatorName will be "streaminglocator-{UUID}".
        "streamingLocatorId": "911b65de-ac92-4391-9aab-80021126d403",
        // (Optional) Name of default ContentKeyPolicy for the StreamingLocator
        "defaultContentKeyPolicyName": "defaultContentKeyPolicy"
    }
Output:
    {
        // Name of the created StreamingLocatorName
        "streamingLocatorName": "streaminglocator-911b65de-ac92-4391-9aab-80021126d403",
        // Name of the created StreamingLocatorId
        "streamingLocatorId": "911b65de-ac92-4391-9aab-80021126d403"
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
    public static class PublishAsset
    {
        [FunctionName("PublishAsset")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - PublishAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.assetName == null)
                return new BadRequestObjectResult("Please pass assetName in the input object");
            if (data.streamingPolicyName == null)
                return new BadRequestObjectResult("Please pass streamingPolicyName in the input object");
            string assetName = data.assetName;
            string streamingPolicyName = data.streamingPolicyName;
            DateTime startDateTime = new DateTime(0);
            if (data.startDateTime != null)
                startDateTime = data.startDateTime;
            DateTime endDateTime = new DateTime(0);
            if (data.endDateTime != null)
                endDateTime = data.endDateTime;
            Guid streamingLocatorId = Guid.NewGuid();
            if (data.streamingLocatorId != null)
                streamingLocatorId = new Guid((string)(data.streamingLocatorId));
            string defaultContentKeyPolicyName = null;
            if (data.defaultContentKeyPolicyName != null)
                defaultContentKeyPolicyName = data.defaultContentKeyPolicyName;
            List<StreamingLocatorUserDefinedContentKey> contentKeys = new List<StreamingLocatorUserDefinedContentKey>();

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string streamingLocatorName = "streaminglocator-" + streamingLocatorId.ToString();
            StreamingLocator streamingLocator = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                streamingLocator = new StreamingLocator()
                {
                    AssetName = assetName,
                    StreamingPolicyName = streamingPolicyName,
                    StartTime = null,
                    EndTime = null,
                    StreamingLocatorId = streamingLocatorId,
                    DefaultContentKeyPolicyName = defaultContentKeyPolicyName,
                };
                if (!startDateTime.Equals(new DateTime(0))) streamingLocator.StartTime = startDateTime;
                if (!endDateTime.Equals(new DateTime(0))) streamingLocator.EndTime = endDateTime;
                streamingLocator.Validate();

                client.StreamingLocators.Create(amsconfig.ResourceGroup, amsconfig.AccountName, streamingLocatorName, streamingLocator);
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
                streamingLocatorName = streamingLocatorName,
                streamingLocatorId = streamingLocator.StreamingLocatorId.ToString()
            });
        }
    }
}
