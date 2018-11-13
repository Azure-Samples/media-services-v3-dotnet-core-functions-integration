//
// Azure Media Services REST API v3 Functions
//
// create-clear-streaming-locator - This function create a clear streaming locator (without DRM)
//
/*
```c#
Input :
{
    "liveEventName": "CH1",
    "storageAccountName" : "" // optional. Specify in which attached storage account the asset should be created. If azureRegion is specified, then the region is appended to the name
    "archiveWindowLength" : 20  // value in minutes, optional. Default is 10 (minutes)
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time
            // Note: the service principal must work with all this accounts
}

Output:
{
  "success": true,
  "operationsVersion": "1.0.0.5",
  "liveEvents": [
    {
      "liveEventName": "CH1",
      "resourceState": "Running",
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
            },
            {
              "streamingLocatorName": "locator-92259edd-db65",
              "streamingPolicyName": "Predefined_ClearStreamingOnly",
              "cencKeyId": null,
              "cbcsKeyId": null,
              "drm": [],
              "urls": [
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest",
                  "protocol": "SmoothStreaming"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-csf)",
                  "protocol": "DashCsf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-cmaf)",
                  "protocol": "DashCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-cmaf)",
                  "protocol": "HlsCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-aapl)",
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
using System.Linq;
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
    public static class CreateClearStreamingLocator
    {
        [FunctionName("create-clear-streaming-locator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            var liveEventName = (string)data.liveEventName;
            if (liveEventName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");

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

            // init default
            var uniquenessAssets = Guid.NewGuid().ToString().Substring(0, 13);

            var streamingLocatorGuid = Guid.NewGuid(); // same locator for the two ouputs if 2 live event namle created
            var uniquenessLocator = streamingLocatorGuid.ToString().Substring(0, 13);
            var streamingLocatorName = "locator-" + uniquenessLocator;

            var manifestName = liveEventName.ToLower();

            var clientTasks = new List<Task<LiveEventEntry>>();

            foreach (var region in azureRegions)
            {
                var task = Task<LiveEventEntry>.Run(async () =>
                {
                    Asset asset = null;
                    LiveOutput liveOutput = null;

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

                    // let's check that the channel exists
                    var liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                    if (liveEvent == null)
                        throw new Exception($"Live event {liveEventName} does not exist.");

                    var outputs =
                        await client.LiveOutputs.ListAsync(config.ResourceGroup, config.AccountName, liveEventName);

                    if (outputs.FirstOrDefault() != null)
                    {
                        liveOutput = outputs.FirstOrDefault();
                        asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName,
                            liveOutput.AssetName);
                    }

                    if (asset == null)
                        throw new Exception("Error - asset not found");

                    try
                    {
                        // streaming locator creation
                        MediaServicesHelpers.LogInformation(log, "Locator creation...", region);

                        var locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset, streamingLocatorGuid);

                        MediaServicesHelpers.LogInformation(log, "locator : " + locator.Name, region);

                        if (liveOutput != null)
                        {
                            await client.Assets.UpdateAsync(config.ResourceGroup, config.AccountName, asset.Name, asset);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("locator creation error", ex);
                    }

                    // object to store the output of the function
                    var generalOutputInfo = new GeneralOutputInfo();

                    // let's build info for the live event and output

                    generalOutputInfo =
                        GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent> { liveEvent });

                    if (!await CosmosHelpers.CreateOrUpdateGeneralInfoDocument(generalOutputInfo.LiveEvents[0]))
                        MediaServicesHelpers.LogWarning(log, "Cosmos access not configured.", region);

                    return generalOutputInfo.LiveEvents[0];

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

            return new OkObjectResult(
                 JsonConvert.SerializeObject(new GeneralOutputInfo { Success = true, LiveEvents = clientTasks.Select(i => i.Result).ToList() }, Formatting.Indented)
            );
        }
    }
}