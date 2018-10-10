//
// Azure Media Services REST API v3 Functions
//
// start-live-event - This function starts a stopped live event
//
/*
```c#
Input :
{
    "liveEventName": "SFPOC",
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Usefull if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
}


Output:
{
  "Success": true,
  "OperationsVersion": "1.0.0.26898",
  "LiveEvents": [
    {
      "Name": "CH1",
      "ResourceState": "Running",
      "VanityUrl": true,
      "Input": [
        {
          "Protocol": "FragmentedMP4",
          "Url": "http://CH1-customerssrlivedeveuwe-euwe.channel.media.azure.net/838afbbac2514fafa2eaed76d8a3cc74/ingest.isml"
        }
      ],
      "InputACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "Preview": [
        {
          "Protocol": "FragmentedMP4",
          "Url": "https://CH1-customerssrlivedeveuwe.preview-euwe.channel.media.azure.net/90083bd1-bed3-4019-9d54-b70e314ac9c8/preview.ism/manifest"
        }
      ],
      "PreviewACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "LiveOutputs": [
        {
          "Name": "output-179744a9-3f6f",
          "ArchiveWindowLength": 120,
          "AssetName": "asset-179744a9-3f6f",
          "AssetStorageAccountName": "rsilsvdeveuwe",
          "ResourceState": "Running",
          "StreamingLocators": [
            {
              "Name": "locator-179744a9-3f6f",
              "StreamingPolicyName": "CH1-321870db-de01",
              "CencKeyId": "58420ba1-da30-4756-b50c-fcd72a9645b7",
              "CbcsKeyId": "ced687fd-c34b-433e-bca7-346a1d7af9f5",
              "Drm": [
                {
                  "Type": "FairPlay",
                  "LicenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/CUSTOMER/getckc?ContentId=CH1&KeyId=ced687fd-c34b-433e-bca7-346a1d7af9f5"
                },
                {
                  "Type": "PlayReady",
                  "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/CUSTOMER/license?ContentId=CH1"
                },
                {
                  "Type": "Widevine",
                  "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/CUSTOMER/license&ContentId=CH1"
                }
              ],
              "Urls": [
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(encryption=cenc)",
                  "Protocol": "SmoothStreaming"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-csf,encryption=cenc)",
                  "Protocol": "DashCsf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-cmaf,encryption=cenc)",
                  "Protocol": "DashCmaf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-cmaf,encryption=cenc)",
                  "Protocol": "HlsCmaf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-aapl,encryption=cenc)",
                  "Protocol": "HlsTs"
                }
              ]
            }
          ]
        }
      ],
      "AMSAccountName": "customerssrlivedeveuwe",
      "Region": "West Europe",
      "ResourceGroup": "GD-INIT-DISTLSV-dev-euwe",
      "id": "customerssrlivedeveuwe:CH1"
    }
  ]
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
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;

namespace LiveDrmOperationsV3
{
    public static class StartChannel
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("start-live-event")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            ConfigWrapper config = null;
            try
            {
                config = new ConfigWrapper(
                     new ConfigurationBuilder()
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

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            LiveEvent liveEvent = null;



            // LIVE EVENT START
            log.LogInformation("Live event starting...");

            try
            {
                // let's check that the channel does not exist already
                liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                if (liveEvent == null)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event not found !");
                }

                await client.LiveEvents.StartAsync(config.ResourceGroup, config.AccountName, liveEventName);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "live event creation error");
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
                var helper = new CosmosHelpers(log,config);
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