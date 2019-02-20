//
// Azure Media Services REST API v3 Functions
//
// Ping - This function returns 200 (used by trafic manager)
//


using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net;

namespace LiveDrmOperationsV3
{
    public static class Ping
    {
        [FunctionName("ping")]
        public static async Task<HttpResponseMessage> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}