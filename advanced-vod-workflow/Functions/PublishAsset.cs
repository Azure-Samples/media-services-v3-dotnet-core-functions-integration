//
// Azure Media Services REST API v3 Functions
//
// PublishAsset - This function publishes the asset (creates a StreamingLocator for the asset).
//
/*
```c#
Input:
    {
        // [Required] The name of the asset used by the streaming locator.
        "assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",

        // [Required] The name of the streaming policy used by the streaming locator.
        // You can either create one with `CreateStreamingPolicy` or use any of the predefined policies:
        //  Predefined_ClearKey,
        //  Predefined_ClearStreamingOnly,
        //  Predefined_DownloadAndClearStreaming,
        //  Predefined_DownloadOnly,
        //  Predefined_MultiDrmCencStreaming,
        //  Predefined_MultiDrmStreaming.
        "streamingPolicyName": "Predefined_ClearStreamingOnly",

        // An alternative media identifier associated with the streaming locator.
        "alternative-media-id": "cid-0001",

        // The default content key policy name used by the streaming locator.
        "contentKeyPolicyName": "defaultContentKeyPolicy",
        
        // JSON string with the content keys to be used by the streaming locator.
        // Use @{url} to load from a file from the specified URL.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streaminglocators/create#streaminglocatorcontentkey.
        "contentKeys": null,

        // The start time (Y-m-d'T'H:M:S'Z') of the streaming locator.
        "startDateTime": "2018-07-01T00:00Z",

        // The end time (Y-m-d'T'H:M:S'Z') of the streaming locator.
        "endDateTime": "2018-12-31T23:59Z",

        // The identifier (UUID) of the streaming locator. "streamingLocatorName" will be "streaminglocator-{UUID}".
        "streamingLocatorId": "911b65de-ac92-4391-9aab-80021126d403",
    }
Output:
    {
        // The name of the created StreamingLocatorName
        "streamingLocatorName": "streaminglocator-911b65de-ac92-4391-9aab-80021126d403",

        // The name of the created StreamingLocatorId
        "streamingLocatorId": "911b65de-ac92-4391-9aab-80021126d403"
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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace advanced_vod_functions_v3
{
    public static class PublishAsset
    {
        [FunctionName("PublishAsset")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - PublishAsset was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.assetName == null)
                return new BadRequestObjectResult("Please pass assetName in the input object");
            if (data.streamingPolicyName == null)
                return new BadRequestObjectResult("Please pass streamingPolicyName in the input object");
            string assetName = data.assetName;
            string streamingPolicyName = data.streamingPolicyName;
            string alternativeMediaId = null;
            if (data.alternativeMediaId != null)
                alternativeMediaId = data.alternativeMediaId;
            string contentKeyPolicyName = null;
            if (data.contentKeyPolicyName != null)
                contentKeyPolicyName = data.contentKeyPolicyName;
            List<StreamingLocatorContentKey> contentKeys = new List<StreamingLocatorContentKey>();
            DateTime startDateTime = new DateTime(0);
            if (data.startDateTime != null)
                startDateTime = data.startDateTime;
            DateTime endDateTime = new DateTime(0);
            if (data.endDateTime != null)
                endDateTime = data.endDateTime;
            Guid streamingLocatorId = Guid.NewGuid();
            if (data.streamingLocatorId != null)
                streamingLocatorId = new Guid((string)(data.streamingLocatorId));
            string streamingLocatorName = "streaminglocator-" + streamingLocatorId.ToString();

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            StreamingLocator streamingLocator = null;
            Asset asset = null;
            StreamingPolicy streamingPolicy = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                asset = client.Assets.Get(amsconfig.ResourceGroup, amsconfig.AccountName, assetName);
                if (asset == null)
                    return new BadRequestObjectResult("Asset not found");
                streamingPolicy = client.StreamingPolicies.Get(amsconfig.ResourceGroup, amsconfig.AccountName, streamingPolicyName);
                if (streamingPolicy == null)
                    return new BadRequestObjectResult("StreamingPolicy not found");
                if (contentKeyPolicyName != null)
                {
                    ContentKeyPolicy contentKeyPolicy = null;
                    contentKeyPolicy = client.ContentKeyPolicies.Get(amsconfig.ResourceGroup, amsconfig.AccountName, contentKeyPolicyName);
                    if (contentKeyPolicy == null)
                        return new BadRequestObjectResult("ContentKeyPolicy not found");
                }
                if (data.contentKeys != null)
                {
                    JsonConverter[] jsonConverters = {
                        new MediaServicesHelperJsonReader()
                    };
                    contentKeys = JsonConvert.DeserializeObject<List<StreamingLocatorContentKey>>(data.contentKeys.ToString(), jsonConverters);
                }

                streamingLocator = new StreamingLocator()
                {
                    AssetName = assetName,
                    StreamingPolicyName = streamingPolicyName,
                    AlternativeMediaId = alternativeMediaId,
                    DefaultContentKeyPolicyName = contentKeyPolicyName,
                    StartTime = null,
                    EndTime = null,
                    StreamingLocatorId = streamingLocatorId,
                };
                if (!startDateTime.Equals(new DateTime(0))) streamingLocator.StartTime = startDateTime;
                if (!endDateTime.Equals(new DateTime(0))) streamingLocator.EndTime = endDateTime;
                if (contentKeys.Count != 0)
                    streamingLocator.ContentKeys = contentKeys;
                streamingLocator.Validate();

                client.StreamingLocators.Create(amsconfig.ResourceGroup, amsconfig.AccountName, streamingLocatorName, streamingLocator);
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
                streamingLocatorId = streamingLocator.StreamingLocatorId.ToString()
            });
        }
    }
}
