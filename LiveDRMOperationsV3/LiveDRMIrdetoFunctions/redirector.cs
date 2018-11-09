//
// Azure Media Services REST API v3 Functions
//
// Redirector - This function reads the Cosmos DB and redirect players to the right output URL
//
// The recirector can be called that way:
// https://myfunctions.azurewebsites.net/api/redirector?channel/mpd (to get drm dash)
// https://myfunctions.azurewebsites.net/api/redirector?channel/m3u8 (to get drm hls)
// https://myfunctions.azurewebsites.net/api/redirector?channel/manifest (to get drm smooth)
//
// clear streams can be reported if application settings "AllowClearStream" is set to "true"
// https://myfunctions.azurewebsites.net/api/redirector?clear/channel/mpd (to get clear dash if such locator exists)
// https://myfunctions.azurewebsites.net/api/redirector?clear/channel/m3u8 (to get drm hls)
// https://myfunctions.azurewebsites.net/api/redirector?clear/channel/manifest (to get drm smooth)
//


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;

namespace LiveDrmOperationsV3
{
    public static class Redirector
    {
        [FunctionName("redirector")]
        public static async Task<HttpResponseMessage> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            bool clear = false;

            log.LogWarning("full path: " + req.RequestUri.PathAndQuery);

            IConfigurationRoot Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddEnvironmentVariables().Build();

            string preferredSE = Config["PreferredStreamingEndpoint"];
            if (preferredSE != null) preferredSE = "https://" + preferredSE;

            var paths = req.RequestUri.PathAndQuery.Split('?').Last().Split('/');

            if ((paths.Count() > 2) && (paths[paths.Count() - 3] == "clear"))  // user wants the clear stream
            {
                if ((Config["AllowClearStream"] != null) && (Config["AllowClearStream"] == "true"))
                {
                    clear = true;
                }
                else
                {
                    return req.CreateResponse(HttpStatusCode.Forbidden);
                }
            }

            var liveEventName = paths[paths.Count() - 2];
            log.LogWarning("eventname: " + liveEventName);

            var format = paths.Last().ToLower();
            log.LogWarning("format: " + format);

            // default settings
            var eventInfoFromCosmos = new List<LiveEventEntry>();

            var urls = new List<UrlEntry>();
            string url = "";
            string protocol = "";

            // Load config from Cosmos
            try
            {
                var liveEvents = (await CosmosHelpers.ReadGeneralInfoDocument(liveEventName));
                if (liveEvents == null) log.LogWarning("Live events not read from Cosmos.");

                var ttt = liveEvents.ToList();

                urls = liveEvents.ToList().Where(l => l.ResourceState == "Running").First()?.LiveOutputs.FirstOrDefault()?.StreamingLocators.Where(l => clear ? l.Drm.Count == 0 : l.Drm.Count > 0)?.FirstOrDefault()?.Urls;

                if (urls == null)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                if (format == "mpd")
                {
                    protocol = OutputProtocol.DashCsf.ToString();
                }
                else if (format == "m3u8")
                {
                    protocol = OutputProtocol.HlsTs.ToString();
                }
                else if (format == "manifest")
                {
                    protocol = OutputProtocol.SmoothStreaming.ToString();
                }

                var urlprotocol = urls.Where(u => u.Protocol == protocol);
                var preferredUrlprotocol = urlprotocol.Where(u => preferredSE != null && u.Url.StartsWith(preferredSE)).FirstOrDefault();
                if (preferredUrlprotocol != null) // preferred SE found
                {
                    url = preferredUrlprotocol.Url;
                }
                else
                {
                    url = urlprotocol.First().Url;
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return req.CreateResponse(HttpStatusCode.InternalServerError, new { error = message });
            }

            //create response
            var response = req.CreateResponse(HttpStatusCode.MovedPermanently);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}