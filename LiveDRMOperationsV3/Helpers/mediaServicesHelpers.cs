using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace LiveDrmOperationsV3.Helpers
{
    internal class MediaServicesHelpers
    {
        /// <summary>
        ///     Create the ServiceClientCredentials object based on the credentials
        ///     supplied in local configuration file.
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
            var clientCredential = new ClientCredential(config.AadClientId, config.AadSecret);
            return await ApplicationTokenProvider.LoginSilentAsync(config.AadTenantId, clientCredential,
                ActiveDirectoryServiceSettings.Azure);
        }

        // </GetCredentialsAsync>

        /// <summary>
        ///     Creates the AzureMediaServicesClient object based on the credentials
        ///     supplied in local configuration file.
        /// </summary>
        /// <param name="config">The parm is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        // <CreateMediaServicesClient>
        public static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
        {
            var credentials = await GetCredentialsAsync(config);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId
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

            var encString = "(encryption=cenc)";
            var encString2 = ",encryption=cenc";

            if (locator.StreamingPolicyName == PredefinedStreamingPolicy.ClearStreamingOnly)
            {
                encString = "";
                encString2 = "";
            }


            // Get the URls to stream the output
            var urls = new List<OutputUrl>();

            foreach (var se in streamingEndpoints)
                if (se.ResourceState == StreamingEndpointResourceState.Running)
                {
                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "https";
                    uriBuilder.Host = se.HostName;
                    uriBuilder.Path = "/" + locator.StreamingLocatorId + "/" + manifestFileName + ".ism/manifest";
                    var myPath = uriBuilder.ToString();
                    if (smoothStreaming)
                        urls.Add(new OutputUrl {Url = myPath + encString, Protocol = OutputtProtocol.SmoothStreaming});
                    if (dashCsf)
                        urls.Add(new OutputUrl
                        {
                            Url = myPath + "(format=mpd-time-csf" + encString2 + ")", Protocol = OutputtProtocol.DashCsf
                        });
                    if (dashCmaf)
                        urls.Add(new OutputUrl
                        {
                            Url = myPath + "(format=mpd-time-cmaf" + encString2 + ")",
                            Protocol = OutputtProtocol.DashCmaf
                        });
                    if (hlsCmaf)
                        urls.Add(new OutputUrl
                        {
                            Url = myPath + "(format=m3u8-cmaf" + encString2 + ")", Protocol = OutputtProtocol.HlsCmaf
                        });
                    if (hlsTs)
                        urls.Add(new OutputUrl
                            {Url = myPath + "(format=m3u8-aapl" + encString2 + ")", Protocol = OutputtProtocol.HlsTs});
                }

            return urls;
        }

        public static List<string> ReturnOutputProtocolsListCbcs(EnabledProtocols protocols)
        {
            var protList = new List<string>();

            if (protocols.Dash) protList.AddRange(new List<string> {OutputtProtocol.DashCmaf.ToString()});
            if (protocols.Hls)
                protList.AddRange(new List<string>
                    {OutputtProtocol.HlsCmaf.ToString(), OutputtProtocol.HlsTs.ToString()});

            return protList;
        }

        public static List<string> ReturnOutputProtocolsListCencPlayReady(EnabledProtocols protocols)
        {
            var protList = new List<string>();

            if (protocols.Dash)
                protList.AddRange(new List<string>
                    {OutputtProtocol.DashCmaf.ToString(), OutputtProtocol.DashCsf.ToString()});
            if (protocols.SmoothStreaming) protList.Add(OutputtProtocol.SmoothStreaming.ToString());

            return protList;
        }

        public static List<string> ReturnOutputProtocolsListCencWidevine(EnabledProtocols protocols)
        {
            var protList = new List<string>();

            if (protocols.Dash)
                protList.AddRange(new List<string>
                    {OutputtProtocol.DashCmaf.ToString(), OutputtProtocol.DashCsf.ToString()});

            return protList;
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