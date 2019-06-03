using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Encoding.Helpers
{
    internal class Helpers
    {
        public static IActionResult ReturnErrorException(ILogger log, Exception ex, string prefixMessage = null)
        {
            var message = "";
            if (ex.GetType() == typeof(ApiErrorException))
            {
                var exapi = (ApiErrorException)ex;
                if (exapi.Response != null)
                    message = exapi.Response.Content;
            }

            return ReturnErrorException(log,
                (prefixMessage == null ? string.Empty : prefixMessage + " : ") + ex.Message + message);
        }

        public static IActionResult ReturnErrorException(ILogger log, string message, string region = null)
        {
            LogError(log, message, region);
            return new BadRequestObjectResult(
                new JObject
                {
                    {"success", false},
                    {"errorMessage", message},
                    {
                        "operationsVersion",
                        AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                    }
                }.ToString());
        }

        public static void LogError(ILogger log, string message, string azureRegion = null)
        {
            log.LogError((azureRegion != null ? "[" + azureRegion + "] " : "") + message);
        }
    }
}