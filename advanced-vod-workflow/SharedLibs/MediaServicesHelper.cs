//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;


namespace advanced_vod_functions_v3.SharedLibs
{
    public class MediaServicesHelper
    {
        public static IAzureMediaServicesClient CreateMediaServicesClientAsync(MediaServicesConfigWrapper config)
        {
            var credentials = GetCredentialsAsync(config).Result;

            return new AzureMediaServicesClient(config.mediaServiceClientCredentials.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId,
            };
        }

        private static async Task<ServiceClientCredentials> GetCredentialsAsync(MediaServicesConfigWrapper config)
        {
            // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symetric key
            ClientCredential clientCredential = new ClientCredential(config.mediaServiceClientCredentials.AadClientId, config.mediaServiceClientCredentials.AadClientSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(config.mediaServiceClientCredentials.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        static public PublishAssetOutput ConvertToPublishAssetOutput(string locatorName, string streamingUrlPrefx, ListPathsResponse paths)
        {
            PublishAssetOutput output = new PublishAssetOutput();

            output.locatorName = locatorName;
            output.streamingUrl = "";
            output.captionVttUrl = "";
            output.annotationsJsonUrl = "";
            output.contentModerationJsonUrl = "";
            output.facesJsonUrl = "";
            output.insightsJsonUrl = "";
            output.ocrJsonUrl = "";

            List<PublishStreamingUrls> psUrls = new List<PublishStreamingUrls>();
            foreach (var path in paths.StreamingPaths)
            {
                var s = new PublishStreamingUrls();
                s.streamingProtocol = path.StreamingProtocol;
                s.encryptionScheme = path.EncryptionScheme;
                s.urls = new string[path.Paths.Count];
                for (int i = 0; i < path.Paths.Count; i++) s.urls[i] = "https://" + streamingUrlPrefx + path.Paths[i];
                if (path.StreamingProtocol == "SmoothStreaming")
                    output.streamingUrl = "https://" + streamingUrlPrefx + path.Paths[0];
                psUrls.Add(s);
            }
            output.streamingUrls = psUrls.ToArray();

            List<string> dUrls = new List<string>();
            foreach (var path in paths.DownloadPaths)
            {
                dUrls.Add("https://" + streamingUrlPrefx + path);
                if (path.EndsWith("annotations.json")) output.annotationsJsonUrl = "https://" + streamingUrlPrefx + path;
                if (path.EndsWith("contentmoderation.json")) output.contentModerationJsonUrl = "https://" + streamingUrlPrefx + path;
                if (path.EndsWith("faces.json")) output.facesJsonUrl = "https://" + streamingUrlPrefx + path;
                if (path.EndsWith("insights.json")) output.insightsJsonUrl = "https://" + streamingUrlPrefx + path;
                if (path.EndsWith("transcript.vtt")) output.captionVttUrl = "https://" + streamingUrlPrefx + path;
            }
            output.downloadUrls = dUrls.ToArray();

            return output;
        }
    }

    public class PublishStreamingUrls
    {
        public string streamingProtocol;
        public string encryptionScheme;
        public string[] urls;
    }

    public class PublishAssetOutput
    {
        public string locatorName;
        public string streamingUrl;
        // Audio Analyzer - VTT (speech-to-text)
        public string captionVttUrl;
        // Video Analyzer JSON
        public string annotationsJsonUrl;
        public string contentModerationJsonUrl;
        public string facesJsonUrl;
        public string insightsJsonUrl;
        public string ocrJsonUrl;
        // URLs
        public PublishStreamingUrls[] streamingUrls;
        public string[] downloadUrls;
    }
}
