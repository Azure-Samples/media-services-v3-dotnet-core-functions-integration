//
// Azure Media Services REST API v3 Functions
//
// delete-live-event-output - this function deletes a live event, and associated live outputs
//
/*
```c#
Input :
{
    "liveEventName":"CH1",
    "deleteAsset" : false // optional, default is True
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, the live event will be deleted from the two regions.
            // Note: the service principal must work with all this accounts
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
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class DeleteChannel
    {
        [FunctionName("delete-live-event-output")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var generalOutputInfos = new List<GeneralOutputInfo>();

            var liveEventName = (string)data.liveEventName;
            if (liveEventName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");

            var deleteAsset = true;
            if (data.deleteAsset != null) deleteAsset = (bool)data.deleteAsset;

            // Azure region management
            var azureRegions = new List<string>();
            if ((string)data.azureRegion != null)
            {
                azureRegions = ((string)data.azureRegion).Split(',').ToList();
            }
            else
            {
                azureRegions.Add((string)null);
            }

            var clientTasks = new List<Task>();

            foreach (var region in azureRegions)
            {
                Task task = Task.Run(async () =>
                   {
                       Task LiveOutputsDeleteTask = null;
                       Task LiveEventStopTask = null;

                       ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddEnvironmentVariables()
                               .Build(),
                               region
                       );

                       MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                       MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                       var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                       // Set the polling interval for long running operations to 2 seconds.
                       // The default value is 30 seconds for the .NET client SDK
                       client.LongRunningOperationRetryTimeout = 2;

                       MediaServicesHelpers.LogInformation(log, "live event : " + liveEventName, region);
                       var liveEvent = client.LiveEvents.Get(config.ResourceGroup, config.AccountName, liveEventName);

                       if (liveEvent == null)
                           throw new Exception($"Live event {liveEventName}  does not exist.");

                       // let's purge all live output for now
                       var ps = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEventName);
                       foreach (var p in ps)
                       {
                           var assetName = p.AssetName;
                           var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, assetName);

                           // let's store name of the streaming policy
                           string streamingPolicyName = null;
                           var streamingLocatorsNames = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, assetName).StreamingLocators.Select(l => l.Name);

                           foreach (var locatorName in streamingLocatorsNames)
                               if (locatorName != null)
                               {
                                   var streamingLocator = await client.StreamingLocators.GetAsync(config.ResourceGroup,
                                       config.AccountName, locatorName);

                                   if (streamingLocator != null) streamingPolicyName = streamingLocator.StreamingPolicyName;
                               }

                           MediaServicesHelpers.LogInformation(log, "deleting live output : " + p.Name, region);
                           LiveOutputsDeleteTask = client.LiveOutputs.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name, p.Name);

                           if (deleteAsset)
                           {
                               MediaServicesHelpers.LogInformation(log, "deleting asset : " + assetName, region);
                               await client.Assets.DeleteAsync(config.ResourceGroup, config.AccountName, assetName);
                               if (streamingPolicyName != null && streamingPolicyName.StartsWith(liveEventName)
                               ) // let's delete the streaming policy if custom
                               {
                                   MediaServicesHelpers.LogInformation(log, "deleting streaming policy : " + streamingPolicyName, region);
                                   await client.StreamingPolicies.DeleteAsync(config.ResourceGroup, config.AccountName,
                                       streamingPolicyName);
                               }
                           }
                       }

                       if (LiveOutputsDeleteTask != null) await LiveOutputsDeleteTask; // live output deletion task

                       if (liveEvent.ResourceState == LiveEventResourceState.Running)
                       {
                           MediaServicesHelpers.LogInformation(log, "stopping live event : " + liveEvent.Name, region);
                           LiveEventStopTask = client.LiveEvents.StopAsync(config.ResourceGroup, config.AccountName, liveEvent.Name);
                       }
                       else if (liveEvent.ResourceState == LiveEventResourceState.Stopping)
                       {
                           var liveevt = liveEvent;
                           while (liveevt.ResourceState == LiveEventResourceState.Stopping)
                           {
                               Thread.Sleep(2000);
                               liveevt = client.LiveEvents.Get(config.ResourceGroup, config.AccountName, liveEvent.Name);
                           }
                       }

                       if (LiveEventStopTask != null) await LiveEventStopTask; // live event stop task

                       MediaServicesHelpers.LogInformation(log, "deleting live event : " + liveEventName, region);
                       await client.LiveEvents.DeleteAsync(config.ResourceGroup, config.AccountName, liveEventName);

                       if (!await CosmosHelpers.DeleteGeneralInfoDocument(new LiveEventEntry
                       {
                           LiveEventName = liveEventName,
                           AMSAccountName = config.AccountName
                       }))
                           MediaServicesHelpers.LogWarning(log, "Error when deleting Cosmos document.", region);

                   });

                clientTasks.Add(task);
            }

            try
            {
                Task.WaitAll(clientTasks.ToArray());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var response = new JObject
            {
                {"liveEventName", liveEventName},
                {"success", true},
                {
                    "operationsVersion",
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                }
            };

            return new OkObjectResult(
                response.ToString()
            );
        }
    }
}