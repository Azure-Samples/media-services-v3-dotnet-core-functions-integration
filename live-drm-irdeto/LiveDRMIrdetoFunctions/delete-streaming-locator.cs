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
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Usefull if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
}

Output:
{
    "Success": true,
    "ErrorMessage" : ""
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
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;


namespace LiveDrmOperationsV3
{
    public static class DeleteStreamingLocator
    {
        [FunctionName("delete-streaming-locator")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            string streamingLocatorName = (string)data.streamingLocatorName;
            if (streamingLocatorName == null)
            {
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass streamingLocatorName in the JSON");
            }

            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(new ConfigurationBuilder()
                                             .SetBasePath(Directory.GetCurrentDirectory())
                                             .AddEnvironmentVariables()
                                             .Build(),
                                              data.azureRegion != null ? (string)data.azureRegion : null
             );
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            log.LogInformation("config loaded.");

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                client.StreamingLocators.Delete(config.ResourceGroup, config.AccountName, streamingLocatorName);

                var response = new JObject()
                                                            {
                                                                { "streamingLocatorName", streamingLocatorName },
                                                                { "Success", true },
                                                                };

                return (ActionResult)new OkObjectResult(
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
