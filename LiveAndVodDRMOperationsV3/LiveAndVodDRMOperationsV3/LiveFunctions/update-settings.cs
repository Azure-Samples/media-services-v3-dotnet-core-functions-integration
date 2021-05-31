//
// Azure Media Services REST API v3 Functions
//
// update-settings - This function creates a document in Cosmos to store the settings for a live event (optional)
//
/*
```c#
Input :
{
    "liveEventName": "TEST",
    "urn": "urn:customer:video:e8927fcf-e1a0-0001-7edd-1eaaaaaa",
    "vendor": "Customer",
    "baseStorageName": "storagename",
    "archiveWindowLength": 10,
    "vanityUrl": true,
    "lowLatency": false,
    "liveEventInputACL": [
        "192.168.0.0/24",
        "86.246.149.14"
    ],
    "liveEventPreviewACL": [
        "192.168.0.0/24",
        "86.246.149.14"
    ],
    "playerJSONData": null
}

Output:
{
    "success": true,
    "errorMessage" : "",
    "operationsVersion": "1.0.0.5"
}


```
*/

using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class UpdateSettings
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("update-settings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            bool success = true;

            var requestBody = new StreamReader(req.Body).ReadToEnd();

            LiveEventSettingsInfo settings = null;
            try
            {
                settings = (LiveEventSettingsInfo)JsonConvert.DeserializeObject(requestBody,
                    typeof(LiveEventSettingsInfo));
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
                    log.LogWarning("Cosmos access not configured or error.");
                    success = false;
                }
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject
            {
                {"Success", success},
                {
                    "OperationsVersion",
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                }
            };

            return new OkObjectResult(
                response.ToString()
            );
        }
    }
}