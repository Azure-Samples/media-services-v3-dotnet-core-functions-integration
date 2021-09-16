using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Common_Utils
{
    class HttpRequest
    {
        private static readonly JsonSerializerSettings SerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Generates the response with HttpStatusCode.OK and JSON body
        /// </summary>
        /// <param name="req">HttpRequestData object</param>
        /// <param name="data">Object to serialize</param>
        /// <returns></returns>
        public static HttpResponseData ResponseOk(HttpRequestData req, object data, HttpStatusCode statuscode = HttpStatusCode.OK)
        {
            var response = req.CreateResponse(statuscode);
            response.Headers.Add("Content-Type", "application/json");
            if (data != null)
            {
                response.WriteString(JsonConvert.SerializeObject(data, SerializerSettings));
            }
            return response;
        }

        /// <summary>
        /// Generates the response with HttpStatusCode.BadRequest and JSON body
        /// </summary>
        /// <param name="req">HttpRequestData object</param>
        /// <param name="message">Error message</param>
        /// <returns></returns>
        public static HttpResponseData ResponseBadRequest(HttpRequestData req, string message)
        {
            dynamic dataNotOk = new JObject();
            dataNotOk.errorMessage = message;

            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "application/json");
            var stringJson = JsonConvert.SerializeObject(dataNotOk, SerializerSettings);
            response.WriteString((string)stringJson);
            return response;
        }
    }
}