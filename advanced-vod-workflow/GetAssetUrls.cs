//
// Azure Media Services REST API v3 Functions
//
// GetAssetUrls - This function provides URLs for the asset.
//
/*
```c#
Input:
    {
        // Name of the Streaming Locator for the asset
        "streamingLocatorName": "streaminglocator-911b65de-ac92-4391-9aab-80021126d403",
        // (Optional) Name of the StreamingEndpoint to be used; "default" is used by default
        "streamingEndpointName": "default",
        // (Optional) Scheme of the streaming URL; "http" or "https", and "https" is used by default
        "streamingUrlScheme": "https"
    }
Output:
    {
        // Path list of Progressive Download
        "downloadPaths": [],
        // Path list of Streaming
        "streamingPaths": [
            {
                // Streaming Protocol
                "StreamingProtocol": "Hls",
                // Encryption Scheme
                "EncryptionScheme": "EnvelopeEncryption",
                // Streaming URL
                "StreamingUrl": "https://amsv3demo-jpea.streaming.media.azure.net/6c4bb037-6907-406d-8e4d-15f91e44ac08/Ignite-short.ism/manifest(format=m3u8-aapl,encryption=cbc)"
            },
            {
                "StreamingProtocol": "Dash",
                "EncryptionScheme": "EnvelopeEncryption",
                "StreamingUrl": "https://amsv3demo-jpea.streaming.media.azure.net/6c4bb037-6907-406d-8e4d-15f91e44ac08/Ignite-short.ism/manifest(format=mpd-time-csf,encryption=cbc)"
            },
            {
                "StreamingProtocol": "SmoothStreaming",
                "EncryptionScheme": "EnvelopeEncryption",
                "StreamingUrl": "https://amsv3demo-jpea.streaming.media.azure.net/6c4bb037-6907-406d-8e4d-15f91e44ac08/Ignite-short.ism/manifest(encryption=cbc)"
            }
        ]
    }

```
*/
//
//

using System;
using System.IO;
using System.Linq;

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
    public static class GetAssetUrls
    {
        [FunctionName("GetAssetUrls")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - GetAssetUrls was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.streamingLocatorName == null)
                return new BadRequestObjectResult("Please pass streamingLocatorName in the input object");
            string streamingLocatorName = data.streamingLocatorName;
            string streamingEndpointName = "default";
            if (data.streamingEndpointName != null)
                streamingEndpointName = data.streamingEndpointName;
            string streamingUrlScheme = "https";
            if (data.streamingUrlScheme != null)
            {
                if (data.streamingUrlScheme.ToString().Equals("http") || data.streamingUrlScheme.ToString().Equals("https"))
                    streamingUrlScheme = data.streamingUrlScheme;
                else
                    return new BadRequestObjectResult("Please pass streamingLocatorName in the input object");
            }

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();

            JArray downloadPaths = new JArray();
            JArray streamingPaths = new JArray();
            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                var paths = client.StreamingLocators.ListPaths(amsconfig.ResourceGroup, amsconfig.AccountName, streamingLocatorName);
                var streamingEndpoint = client.StreamingEndpoints.Get(amsconfig.ResourceGroup, amsconfig.AccountName, streamingEndpointName);

                foreach (var path in paths.DownloadPaths)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = streamingUrlScheme;
                    uriBuilder.Host = streamingEndpoint.HostName;
                    uriBuilder.Path = path;
                    downloadPaths.Add(uriBuilder.ToString());
                }

                foreach (var path in paths.StreamingPaths)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = streamingUrlScheme;
                    uriBuilder.Host = streamingEndpoint.HostName;

                    if (path.Paths.Count > 0)
                    {
                        uriBuilder.Path = path.Paths[0];
                        JObject p = new JObject();
                        p["StreamingProtocol"] = path.StreamingProtocol.ToString();
                        p["EncryptionScheme"] = path.EncryptionScheme.ToString();
                        p["StreamingUrl"] = uriBuilder.ToString();
                        streamingPaths.Add(p);
                    }
                }
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

            JObject result = new JObject();
            result["downloadPaths"] = downloadPaths;
            result["streamingPaths"] = streamingPaths;
            return (ActionResult)new OkObjectResult(result);
        }
    }
}
