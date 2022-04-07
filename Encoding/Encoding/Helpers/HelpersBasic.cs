using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Encoding.Helpers
{
    internal class HelpersBasic
    {
        public static IActionResult ReturnErrorException(ILogger log, Exception ex, string prefixMessage = null)
        {
            var message = "";
            
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