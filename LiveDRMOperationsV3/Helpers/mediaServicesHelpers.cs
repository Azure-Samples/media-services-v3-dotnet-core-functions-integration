using Microsoft.Azure.Management.Media;
using Microsoft.Rest;
using System.Threading.Tasks;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections.Generic;
using System;
using Microsoft.Azure.Management.Media.Models;

namespace LiveDrmOperationsV3.Helpers
{
    class MediaServicesHelpers
    {
        /// <summary>
        /// Create the ServiceClientCredentials object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        // <GetCredentialsAsync>
        public static async Task<ServiceClientCredentials> GetCredentialsAsync(ConfigWrapper config)
        {
            // Use UserTokenProvider.LoginWithPromptAsync or UserTokenProvider.LoginSilentAsync to get a token using user authentication
            //// ActiveDirectoryClientSettings.UsePromptOnly
            //// UserTokenProvider.LoginWithPromptAsync

            // Use ApplicationTokenProvider.LoginSilentWithCertificateAsync or UserTokenProvider.LoginSilentAsync to get a token using service principal with certificate
            //// ClientAssertionCertificate
            //// ApplicationTokenProvider.LoginSilentWithCertificateAsync

            // Use ApplicationTokenProvider.LoginSilentAsync to get a token using a service principal with symetric key
            ClientCredential clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential, ActiveDirectoryServiceSettings.Azure);
        }

        // </GetCredentialsAsync>

        /// <summary>
        /// Creates the AzureMediaServicesClient object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        // <CreateMediaServicesClient>
        public static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
        {
            var credentials = await GetCredentialsAsync(config);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId,
            };
        }
        // </CreateMediaServicesClient>


        public static List<OutputUrl> GetUrls(ConfigWrapper config,
                                   IAzureMediaServicesClient client,
                                   StreamingLocator locator,
                                   string manifestFileName,
                                   bool smoothStreaming = true,
                                   bool dashCsf = true,
                                   bool hlsTs = true,
                                   bool dashCmaf = true,
                                   bool hlsCmaf = true
   )
        {
            var streamingEndpoints = client.StreamingEndpoints.List(config.ResourceGroup, config.AccountName);

            string encString = "(encryption=cenc)";
            string encString2 = ",encryption=cenc";

            if (locator.StreamingPolicyName == PredefinedStreamingPolicy.ClearStreamingOnly)
            {
                encString = "";
                encString2 = "";
            }


            // Get the URls to stream the output
            List<OutputUrl> urls = new List<OutputUrl>();

            foreach (var se in streamingEndpoints)
            {
                if (se.ResourceState == StreamingEndpointResourceState.Running)
                {
                    UriBuilder uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "https";
                    uriBuilder.Host = se.HostName;
                    uriBuilder.Path = "/" + locator.StreamingLocatorId + "/" + manifestFileName + ".ism/manifest";
                    var myPath = uriBuilder.ToString();
                    if (smoothStreaming) urls.Add(new OutputUrl() { Url = myPath + encString, Protocol = OutputtProtocol.SmoothStreaming });
                    if (dashCsf) urls.Add(new OutputUrl() { Url = myPath + "(format=mpd-time-csf" + encString2 + ")", Protocol = OutputtProtocol.DashCsf });
                    if (dashCmaf) urls.Add(new OutputUrl() { Url = myPath + "(format=mpd-time-cmaf" + encString2 + ")", Protocol = OutputtProtocol.DashCmaf });
                    if (hlsCmaf) urls.Add(new OutputUrl() { Url = myPath + "(format=m3u8-cmaf" + encString2 + ")", Protocol = OutputtProtocol.HlsCmaf });
                    if (hlsTs) urls.Add(new OutputUrl() { Url = myPath + "(format=m3u8-aapl" + encString2 + ")", Protocol = OutputtProtocol.HlsTs });
                }
            }

            return urls;
        }

        public static List<string> ReturnOutputProtocolsListCbcs(EnabledProtocols protocols)
        {
            var protLict = new List<string>();

            if (protocols.Dash) protLict.AddRange(new List<string>() { OutputtProtocol.DashCmaf.ToString() });
            if (protocols.Hls) protLict.AddRange(new List<string>() { OutputtProtocol.HlsCmaf.ToString(), OutputtProtocol.HlsTs.ToString() });

            return protLict;
        }

        public static List<string> ReturnOutputProtocolsListCencPlayReady(EnabledProtocols protocols)
        {
            var protLict = new List<string>();

            if (protocols.Dash) protLict.AddRange(new List<string>() { OutputtProtocol.DashCmaf.ToString(), OutputtProtocol.DashCsf.ToString() });
            if (protocols.SmoothStreaming) protLict.Add(OutputtProtocol.SmoothStreaming.ToString());

            return protLict;
        }

        public static List<string> ReturnOutputProtocolsListCencWidevine(EnabledProtocols protocols)
        {
            var protLict = new List<string>();

            if (protocols.Dash) protLict.AddRange(new List<string>() { OutputtProtocol.DashCmaf.ToString(), OutputtProtocol.DashCsf.ToString() });

            return protLict;
        }
    }


    public class OutputUrl
    {
        public string Url { get; set; }
        public OutputtProtocol Protocol { get; set; }
    }

    public enum OutputtProtocol
    {
        SmoothStreaming,
        DashCsf,
        DashCmaf,
        HlsCmaf,
        HlsTs
    }

}
