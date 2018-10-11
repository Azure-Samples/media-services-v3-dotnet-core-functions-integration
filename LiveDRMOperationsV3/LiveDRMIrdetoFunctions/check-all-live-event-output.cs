//
// Azure Media Services REST API v3 Functions
//
// check-all-live-event-output - This function lists all live events and live outputs
//
/*
```c#
Input : (can be empty)
{
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Usefull if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
}

output :
if no live event:
{

  "Success": true,

  "LiveEvents": []

}

with some live events:
{
  "Success": true,
  "OperationsVersion": "1.0.0.1",
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
                  "LicenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/CUSTOMER/getckc?ContentId=CH1&KeyId=ced687fd-c34b-433e-bca7-346a1d7af9f5",
                  "Protocols": [
                    "DashCmaf",
                    "HlsCmaf",
                    "HlsTs"
                  ]
                },
                {
                  "Type": "PlayReady",
                  "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/CUSTOMER/license?ContentId=CH1",
                  "Protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
                },
                {
                  "Type": "Widevine",
                  "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/CUSTOMER/license&ContentId=CH1",
                  "Protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
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
            },
            {
              "Name": "locator-92259edd-db65",
              "StreamingPolicyName": "Predefined_ClearStreamingOnly",
              "CencKeyId": null,
              "CbcsKeyId": null,
              "Drm": [],
              "Urls": [
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest",
                  "Protocol": "SmoothStreaming"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-csf)",
                  "Protocol": "DashCsf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-cmaf)",
                  "Protocol": "DashCmaf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-cmaf)",
                  "Protocol": "HlsCmaf"
                },
                {
                  "Url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-aapl)",
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
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Models;
using LiveDrmOperationsV3.Helpers;
using System.Collections.Generic;

namespace LiveDrmOperationsV3
{
    public static class CheckChannels
    {

        [FunctionName("check-all-live-event-output")]
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
                return IrdetoHelpers.ReturnErrorException(log, ex, "Error");
            }

            log.LogInformation("config loaded.");

            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            var liveEvents = client.LiveEvents.List(config.ResourceGroup, config.AccountName);

            // object to store the output of the function
            var generalOutputInfo = new GeneralOutputInfo();

            // let's list live events
            try
            {
                generalOutputInfo = GenerateInfoHelpers.GenerateOutputInformation(config, client, liveEvents.ToList());
            }

            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            try
            {
                List<bool> success = new List<bool>();
                foreach (var le in generalOutputInfo.LiveEvents)
                {
                    success.Add(await CosmosHelpers.CreateOrUpdateGeneralInfoDocument(le));
                }

                if (success.Any(b => b == false))
                {
                    log.LogWarning("Cosmos access not configured.");
                }

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