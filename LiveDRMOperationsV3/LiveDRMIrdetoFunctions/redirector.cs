//
// Azure Media Services REST API v3 Functions
//
// Redirector - This function reads the Cosmos DB and redirect players to the right output URL
//
// A proxies.json should be used jointly with the function. In that case, the recirector will be called :
// https://redirector-euno.azurewebsites.net/live?/channel/mpd (to get dash)
// https://redirector-euno.azurewebsites.net/live?/channel/m3u8 (to get hls)
//
//
// proxies.json
//
/*
```c#
{
    "$schema": "http://json.schemastore.org/proxies",
  "proxies": {
    "proxy1": {
      "matchCondition": {
        "methods": [ "GET" ],
        "route": "/live"
      },
      "backendUri": "https://redirector-euno.azurewebsites.net/api/redirector?code=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX=="
    }
  }
}
```
*/

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
             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            //string OriginUrl = req.Headers.GetValues("DISGUISED-HOST").FirstOrDefault();
            //log.LogInformation("RequestURI org: " + OriginUrl);

            log.LogWarning("full path: " + req.RequestUri.PathAndQuery);

            IConfigurationRoot Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddEnvironmentVariables().Build();

            string preferredSE = Config["PreferredStreamingEndpoint"];
            if (preferredSE != null) preferredSE = "https://" + preferredSE;

            var paths = req.RequestUri.PathAndQuery.Split('?').Last().Split('/');
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

                urls = liveEvents.ToList().Where(l => l.ResourceState == "Running").First()?.LiveOutputs.First()?.StreamingLocators.Where(l => l.Drm.Count > 0).First()?.Urls;

                if (format == "mpd")
                {
                    protocol = "DashCsf";
                }
                else if (format == "m3u8")
                {
                    protocol = "HlsTs";
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
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            //create response
            var response = req.CreateResponse(HttpStatusCode.MovedPermanently);
            response.Headers.Location = new Uri(url);
            return response;
        }
    }
}