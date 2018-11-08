//
// Azure Media Services REST API v3 Functions
//
// stop-live-event - This function stops a running live event
//
/*
```c#
Input :
{
    "liveEventName": "SFPOC",
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty. This feature is useful if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
}


Output:
{
  "success": true,
  "operationsVersion": "1.0.0.5",
  "liveEvents": [
    {
      "liveEventName": "CH1",
      "resourceState": "Stopped",
      "vanityUrl": true,
      "amsAccountName": "customerssrlivedeveuwe",
      "region": "West Europe",
      "resourceGroup": "GD-INIT-DISTLSV-dev-euwe",
      "lowLatency": false,
      "id": "customerssrlivedeveuwe:CH1",
      "input": [
        {
          "protocol": "FragmentedMP4",
          "url": "http://CH1-customerssrlivedeveuwe-euwe.channel.media.azure.net/838afbbac2514fafa2eaed76d8a3cc74/ingest.isml"
        }
      ],
      "inputACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "preview": [
        {
          "protocol": "FragmentedMP4",
          "url": "https://CH1-customerssrlivedeveuwe.preview-euwe.channel.media.azure.net/90083bd1-bed3-4019-9d54-b70e314ac9c8/preview.ism/manifest"
        }
      ],
      "previewACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "liveOutputs": [
        {
          "liveOutputName": "output-179744a9-3f6f",
          "archiveWindowLength": 120,
          "assetName": "asset-179744a9-3f6f",
          "assetStorageAccountName": "rsilsvdeveuwe",
          "resourceState": "Running",
          "streamingLocators": [
            {
              "streamingLocatorName": "locator-179744a9-3f6f",
              "streamingPolicyName": "CH1-321870db-de01",
              "cencKeyId": "58420ba1-da30-4756-b50c-fcd72a9645b7",
              "cbcsKeyId": "ced687fd-c34b-433e-bca7-346a1d7af9f5",
              "drm": [
                {
                  "type": "FairPlay",
                  "licenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/CUSTOMER/getckc?ContentId=CH1&KeyId=ced687fd-c34b-433e-bca7-346a1d7af9f5",
                  "protocols": [
                    "DashCmaf",
                    "HlsCmaf",
                    "HlsTs"
                  ]
                },
                {
                  "type": "PlayReady",
                  "licenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/CUSTOMER/license?ContentId=CH1",
                  "protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
                },
                {
                  "type": "Widevine",
                  "licenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/CUSTOMER/license&ContentId=CH1",
                  "protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
                }
              ],
              "urls": [
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(encryption=cenc)",
                  "protocol": "SmoothStreaming"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-csf,encryption=cenc)",
                  "protocol": "DashCsf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-cmaf,encryption=cenc)",
                  "protocol": "DashCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-cmaf,encryption=cenc)",
                  "protocol": "HlsCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-aapl,encryption=cenc)",
                  "protocol": "HlsTs"
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

namespace LiveDrmOperationsV3
{
    public static class StopChannel
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("stop-live-event")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

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

            var liveEventName = (string)data.liveEventName;
            if (liveEventName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");

            var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            LiveEvent liveEvent = null;


            // LIVE EVENT START
            log.LogInformation("Live event stopping...");

            try
            {
                // let's check that the channel does not exist already
                liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                if (liveEvent == null) return IrdetoHelpers.ReturnErrorException(log, "Error : live event not found !");
                if (liveEvent.ResourceState == LiveEventResourceState.Stopped) return IrdetoHelpers.ReturnErrorException(log, "Error : live event already stopped !");

                await client.LiveEvents.StopAsync(config.ResourceGroup, config.AccountName, liveEventName);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "live event stopping error");
            }


            // object to store the output of the function
            var generalOutputInfo = new GeneralOutputInfo();

            // let's build info for the live event and output
            try
            {
                generalOutputInfo =
                    GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent> { liveEvent });
            }

            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            try
            {
                if (!await CosmosHelpers.CreateOrUpdateGeneralInfoDocument(generalOutputInfo.LiveEvents[0]))
                    log.LogWarning("Cosmos access not configured.");
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            return new OkObjectResult(
                JsonConvert.SerializeObject(generalOutputInfo, Formatting.Indented)
            );
        }
    }
}