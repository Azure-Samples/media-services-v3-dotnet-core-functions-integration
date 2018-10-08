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


```
*/
//
//

using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;

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
                var helper = new CosmosHelpers(log,config);
                await helper.CreateOrUpdateSettingsDocument(settings);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject()
                                                            {

                                                                { "Success", true },
                                                                };

            return (ActionResult)new OkObjectResult(
                   response.ToString()
                    );
        }
    }
}