//
// Azure Media Services REST API v3 Functions
//
// Redirector - This function reads the Cosmos DB and redirect players to the existing output URL
// it read output info from Cosmos to get URLs.
// as an option, il also read Cosmos event settings or streaming endpoint selection. In the live event settings, it uses the following data to do load balancing :
/*
```c#
    "redirectorStreamingEndpointData":[
        {"StreamingEndpointName":"verizon", "Percentage":"50"},
        {"StreamingEndpointName":"akamai", "Percentage":"50"}
       ]
```

 The redirector can be called that way:
 https://myfunctions.azurewebsites.net/api/redirector?channel/mpd (to get drm dash)
 https://myfunctions.azurewebsites.net/api/redirector?channel/m3u8 (to get drm hls)
 https://myfunctions.azurewebsites.net/api/redirector?channel/manifest (to get drm smooth)

 clear streams can be reported if application settings "AllowClearStream" is set to "true"
 https://myfunctions.azurewebsites.net/api/redirector?clear/channel/mpd (to get clear dash if such locator exists)
 https://myfunctions.azurewebsites.net/api/redirector?clear/channel/m3u8 (to get clear hls)
 https://myfunctions.azurewebsites.net/api/redirector?clear/channel/manifest (to get clear smooth)

*/

using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class Redirector
    {
        private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddEnvironmentVariables()
         .Build();

        private static readonly bool AllowClear = bool.Parse(Config["AllowClearStream"] ?? false.ToString());


        [FunctionName("redirector")]
        public static async Task<HttpResponseMessage> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            bool clear = false;
            string preferredSE = null;

            log.LogWarning("full path: " + req.RequestUri.PathAndQuery);

            // lets get data from the url
            var paths = req.RequestUri.PathAndQuery.Split('?').Last().Split('/');

            if ((paths.Count() > 2) && (paths[paths.Count() - 3] == "clear"))  // user wants the clear stream
            {
                if (AllowClear)
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

            // let's read balancing preferences and choose SE
            try
            {
                var liveEventSettings = (await CosmosHelpers.ReadSettingsDocument(liveEventName));
                if (liveEventSettings == null)
                {
                    log.LogWarning("Live event settings not read from Cosmos.");
                }

                // let' choose the preferred se
                else if (liveEventSettings.RedirectorStreamingEndpointData != null) // list of preferred se with percentage
                {
                    int randomPercentage = (new Random()).Next(0, 100);
                    int value = 0;
                    foreach (var se in liveEventSettings.RedirectorStreamingEndpointData)
                    {
                        value += se.Percentage;
                        if (randomPercentage <= value)
                        {
                            preferredSE = "https://" + se.StreamingEndpointName; // we select this one
                            break;
                        }
                    }
                }

                var liveEvents = (await CosmosHelpers.ReadGeneralInfoDocument(liveEventName));
                if (liveEvents == null) log.LogWarning("Live events not read from Cosmos.");

                if (liveEvents.Count() == 0)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                urls = liveEvents
                    .ToList()
                    .Where(l => l.ResourceState == "Running")?
                    .First()?
                    .LiveOutputs?
                    .FirstOrDefault()?
                    .StreamingLocators?
                    .Where(l => clear ? l.Drm.Count == 0 : l.Drm.Count > 0)?
                    .FirstOrDefault()?
                    .Urls;

                if (urls == null || urls.Count == 0)
                {
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                switch (format)
                {
                    case "mpd":
                        protocol = OutputProtocol.DashCsf.ToString();
                        break;

                    case "m3u8":
                        protocol = OutputProtocol.HlsTs.ToString();
                        break;

                    case "manifest":
                        protocol = OutputProtocol.SmoothStreaming.ToString();
                        break;

                    default:
                        break;
                }

                var urlprotocol = urls.Where(u => u.Protocol == protocol);
                var preferredUrlprotocol = urlprotocol.Where(u => preferredSE != null && u.Url.StartsWith(preferredSE)).FirstOrDefault();

                url = (preferredUrlprotocol != null) ? preferredUrlprotocol.Url : urlprotocol.First().Url; // if preferredse found otherwise let's take first
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return req.CreateResponse(HttpStatusCode.InternalServerError, new
                {
                    error = message
                });
            }

            //create response
            var response = req.CreateResponse(HttpStatusCode.MovedPermanently);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}