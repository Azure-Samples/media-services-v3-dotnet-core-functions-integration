/*
Input :
{
"streamingLocatorName": ""
}

*/



using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;

namespace ImgDrmOperationsV2
{
    public static class DeleteStreamingLocator
    {
        [FunctionName("delete-streaming-locator")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            string streamingLocatorName = (string)data.streamingLocatorName;
            if (streamingLocatorName == null)
                return new BadRequestObjectResult("Error - please pass locator name in the JSON");

            

            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddEnvironmentVariables()
             .Build());
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error - " + ex.Message);
            }

            log.Info("config loaded.");

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                client.StreamingLocators.Delete(config.ResourceGroup, config.AccountName, streamingLocatorName);
                

                return (ActionResult)new OkObjectResult($"ok");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error - " + ex.Message);
            }
        }
    }
}
