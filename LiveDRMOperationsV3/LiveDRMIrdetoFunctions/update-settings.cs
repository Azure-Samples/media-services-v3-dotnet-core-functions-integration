//
// Azure Media Services REST API v3 Functions
//
// update-settings - This function creates a document in Cosmos to store the settings for a live event (optional)
//
/*
```c#
Input :
{
}

Output:
{
    "Success": true,
    "ErrorMessage" : "",
    "OperationsVersion": "1.0.0.26898"
}


```
*/
//
//

using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using System.Reflection;

namespace LiveDrmOperationsV3
{
    public static class UpdateSettings
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("update-settings")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            // dynamic data = JsonConvert.DeserializeObject(requestBody);
            LiveEventSettingsInfo settings = null;
            try
            {
                settings = (LiveEventSettingsInfo)JsonConvert.DeserializeObject(requestBody, typeof(LiveEventSettingsInfo));
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }


            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(
                     new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                     .AddEnvironmentVariables()
                     .Build()

             );
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            log.LogInformation("config loaded.");



            try
            {
                if (!await CosmosHelpers.CreateOrUpdateSettingsDocument(settings))
                {
                    log.LogWarning("Cosmos access not configured.");
                }
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject()
                                                            {
                                                                { "Success", true },
                                                                { "OperationsVersion", AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString() }
                                                                };

            return (ActionResult)new OkObjectResult(
                   response.ToString()
                    );
        }
    }
}