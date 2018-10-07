//
// Azure Media Services REST API v3 Functions
//
// delete-live-event-output - this function deletes a live event, and associated live outputs
//
/*
```c#
Input :
{
    "liveEventName":"FPOC",
    "deleteAsset" : false // optional, default is True
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
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using LiveDRMIrdeto.Models;
using LiveDRMIrdeto.Helpers;

namespace LiveDrmOperationsV3
{
    public static class DeleteChannel
    {
        [FunctionName("delete-live-event-output")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

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

            string liveEventName = (string)data.liveEventName;
            if (liveEventName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");

            bool deleteAsset = true;
            if (data.deleteAsset != null)
            {
                deleteAsset = (bool)data.deleteAsset;
            }

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                log.LogInformation("live event : " + liveEventName);
                var liveEvent = client.LiveEvents.Get(config.ResourceGroup, config.AccountName, liveEventName);

                if (liveEvent == null)
                {
                    return IrdetoHelpers.ReturnErrorException(log, $"Live event {liveEventName}  does not exist.");
                }

                // let's purge all live output for now

                var ps = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEventName);
                foreach (var p in ps)
                {
                    string assetName = p.AssetName;

                    // let's store name of the streaming policy
                    string streamingPolicyName = null;
                    var streamingLocatorName = IrdetoHelpers.ReturnLocatorNameFromDescription(p);
                    if (streamingLocatorName != null)
                    {
                        var streamingLocator = client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, streamingLocatorName);
                        if (streamingLocator != null)
                        {
                            streamingPolicyName = streamingLocator.StreamingPolicyName;
                        }
                    }
                    log.LogInformation("deleting live output : " + p.Name);
                    await client.LiveOutputs.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name, p.Name);
                    if (deleteAsset)
                    {
                        log.LogInformation("deleting asset : " + assetName);
                        client.Assets.DeleteAsync(config.ResourceGroup, config.AccountName, assetName);
                        if (streamingPolicyName != null) // let's delete the streaming policy
                        {
                            log.LogInformation("deleting streaming policy : " + streamingPolicyName);
                            client.StreamingPolicies.DeleteAsync(config.ResourceGroup, config.AccountName, streamingPolicyName);
                        }
                    }
                }
                if (liveEvent.ResourceState == LiveEventResourceState.Running)
                {
                    log.LogInformation("stopping live event : " + liveEvent.Name);
                    await client.LiveEvents.StopAsync(config.ResourceGroup, config.AccountName, liveEvent.Name);
                }
                else if (liveEvent.ResourceState == LiveEventResourceState.Stopping)
                {
                    var liveevt = liveEvent;
                    while (liveevt.ResourceState == LiveEventResourceState.Stopping)
                    {
                        System.Threading.Thread.Sleep(2000);
                        liveevt = client.LiveEvents.Get(config.ResourceGroup, config.AccountName, liveEvent.Name);
                    }
                }

                log.LogInformation("deleting live event : " + liveEvent.Name);
                await client.LiveEvents.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name);
            }

            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }


            try
            {
                var helper = new CosmosHelpers(log, config);
                await helper.DeleteGeneralInfoDocument(new LiveEventEntry() { Name = liveEventName, AMSAccountName = config.AccountName });
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject()
                                                            {
                                                                { "LiveEventName", liveEventName },
                                                                { "Success", true },
                                                                };

            return (ActionResult)new OkObjectResult(
                   response.ToString()
                    );
        }
    }
}
