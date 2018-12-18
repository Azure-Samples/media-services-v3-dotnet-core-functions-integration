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

using Newtonsoft.Json.Linq;


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

        public static JToken ConvertTypeInMediaServicesJson(string jsonString)
        {
            JToken token = JToken.Parse(jsonString);
            foreach (JToken t in token.FindTokens("$type"))
            {
                JValue jval = (JValue)token.SelectToken(t.Path);
                string[] typeInfo = ((string)jval.Value).Split(',');
                string[] className = typeInfo[0].Split('.');
                string val = "#" + "Microsoft.Media." + className[className.Length - 1];
                JProperty jprop = (JProperty)t.Parent;
                jprop.AddAfterSelf(new JProperty("@odata.type", val));
                t.Parent.Remove();
            }
            return token;
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
                PublishStreamingUrls s = new PublishStreamingUrls();
                s.streamingProtocol = path.StreamingProtocol.ToString();
                s.encryptionScheme = path.EncryptionScheme.ToString();
                s.urls = new string[path.Paths.Count];
                for (int i = 0; i < path.Paths.Count; i++) s.urls[i] = "https://" + streamingUrlPrefx + path.Paths[i];
                if (path.StreamingProtocol.ToString() == "SmoothStreaming")
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

    public static class JsonExtensions
    {
        public static List<JToken> FindTokens(this JToken containerToken, string name)
        {
            List<JToken> matches = new List<JToken>();
            FindTokens(containerToken, name, matches);
            return matches;
        }

        private static void FindTokens(JToken containerToken, string name, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        matches.Add(child.Value);
                    }
                    FindTokens(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches);
                }
            }
        }
    }
}
