//
// Azure Media Services REST API v3 Functions
//
// delete-streaming-locator - This function delete a a streaming locator (to unpublish)
//
/*
```c#
Input :
{
    "streamingLocatorName": "locator-c03de9fe-dd04",
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty. This feature is useful if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
}

Output:
{
    "Success": true,
    "ErrorMessage" : "",
    "OperationsVersion": "1.0.0.1"
}


```
*/
//
//


using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LiveDrmOperationsV3
{
    public static class DeleteStreamingLocator
    {
        [FunctionName("delete-streaming-locator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            var streamingLocatorName = (string)data.streamingLocatorName;
            if (streamingLocatorName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass streamingLocatorName in the JSON");

            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .Build(),
                        (string)data.azureRegion
                );
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            log.LogInformation("config loaded.");
            log.LogInformation("connecting to AMS account : " + config.AccountName);

            var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                client.StreamingLocators.Delete(config.ResourceGroup, config.AccountName, streamingLocatorName);

                var response = new JObject
                {
                    {"streamingLocatorName", streamingLocatorName},
                    {"Success", true},
                    {
                        "OperationsVersion",
                        AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                    }
                };

                return new OkObjectResult(
                    response.ToString()
                );
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }
        }
    }
}