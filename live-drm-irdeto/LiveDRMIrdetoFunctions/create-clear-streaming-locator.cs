//
// Azure Media Services REST API v3 Functions
//
// create-clear-streaming-locator - This function create a clear streaming locator (without DRM)
//
/*
```c#
Input :
{
    "liveEventName": "SFPOC",
    "storageAccountName" : "" // optional. Specify in which attached storage account the asset should be created. If azureRegion is specified, then the region is appended to the name
    "archiveWindowLength" : 20  // value in minutes, optional. Default is 10 (minutes)
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
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;

namespace LiveDrmOperationsV3
{
    public static class CreateClearStreamingLocator
    {
        [FunctionName("create-clear-streaming-locator")]
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
            {
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");
            }

            // default settings
            LiveEventSettingsInfo eventInfoFromCosmos = new LiveEventSettingsInfo()
            {
                liveEventName = liveEventName
            };

            // Load config from Cosmos
            try
            {
                var helper = new CosmosHelpers(log, config);
                eventInfoFromCosmos = await helper.ReadSettingsDocument(liveEventName) ?? eventInfoFromCosmos;
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }


            // init default
            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            string streamingLocatorName = "locator-" + uniqueness;
            string manifestName = liveEventName.ToLower();

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            Asset asset = null;
            LiveEvent liveEvent = null;
            LiveOutput liveOutput = null;

            if (data.archiveWindowLength != null)
            {
                eventInfoFromCosmos.archiveWindowLength = (int)data.archiveWindowLength;
            }

            try
            {
                // let's check that the channel exists
                liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                if (liveEvent == null)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event does not exist !");
                }

                if (liveEvent.ResourceState != LiveEventResourceState.Running)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event is not running !");
                }

                var outputs = await client.LiveOutputs.ListAsync(config.ResourceGroup, config.AccountName, liveEventName);

                if (outputs.FirstOrDefault() != null)
                {
                    liveOutput = outputs.FirstOrDefault();
                    asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName, liveOutput.AssetName);
                }
                if (asset == null)
                {
                    return IrdetoHelpers.ReturnErrorException(log, $"Error - asset not found");
                }
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            try
            {
                // streaming locator creation
                log.LogInformation("Locator creation...");

                StreamingLocator locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset);

                log.LogInformation("locator : " + locator.Name);

                if (liveOutput != null)
                {

                    asset.Description = IrdetoHelpers.SetLocatorNameInDescription(streamingLocatorName, asset.Description);

                    await client.Assets.UpdateAsync(config.ResourceGroup, config.AccountName, asset.Name, asset);
                }

            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "locator creation error");
            }


            // object to store the output of the function
            var generalOutputInfo = new GeneralOutputInfo();

            // let's build info for the live event and output
            try
            {
                generalOutputInfo = GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent>() { liveEvent });
            }

            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            try
            {
                var helper = new CosmosHelpers(log, config);
                await helper.CreateOrUpdateGeneralInfoDocument(generalOutputInfo.LiveEvents[0]);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            return (ActionResult)new OkObjectResult(
             JsonConvert.SerializeObject(generalOutputInfo, Formatting.Indented)
               );

        }
    }
}
