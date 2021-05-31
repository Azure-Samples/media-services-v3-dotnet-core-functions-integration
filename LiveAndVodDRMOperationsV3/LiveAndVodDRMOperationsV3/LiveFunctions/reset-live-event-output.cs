//
// Azure Media Services REST API v3 Functions
//
// reset-live-event-output - This function resets a new live event and output (to be used with Irdeto)
// keys are reused when possible, streaming policies are reused
// locator GUID is new (so output URLs will change)
//
// if live event is stopped, then it will still "reset" the live output (recreate a new one)
/*
```c#
Input :
{
    "liveEventName": "CH1",
    "deleteAsset" : false, // optional, default is True - if asset is not deleted then we cannot reuse the keys for the new asset. New keys will be used for the new asset.
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. This function will reset the live event in both regions.
            // Note: the service principal must work with all this accounts
    "archiveWindowLength" : 20  // value in minutes, optional. Default is 10 (minutes)
}



Output:
{
  "success": true,
  "operationsVersion": "1.0.0.5",
  "liveEvents": [
    {
      "liveEventName": "CH1",
      "resourceState": "Running",
      "useStaticHostname": true,
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class ResetChannel
    {
        private const string labelCenc = "cencDefaultKey";
        private const string labelCbcs = "cbcsDefaultKey";
        private const string assetprefix = "nb:cid:UUID:";

        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("reset-live-event-output")]
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

            // default settings
            var eventInfoFromCosmos = new LiveEventSettingsInfo
            {
                LiveEventName = liveEventName
            };

            // Load config from Cosmos
            try
            {
                var setting = await CosmosHelpers.ReadSettingsDocument(liveEventName);
                eventInfoFromCosmos = setting ?? eventInfoFromCosmos;

                if (setting == null) log.LogWarning("Settings not read from Cosmos.");
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

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
            var deleteAsset = (data.deleteAsset != null) ? (bool)data.deleteAsset : true;

            var uniquenessAssets = Guid.NewGuid().ToString().Substring(0, 13);

            string uniquenessPolicyName = Guid.NewGuid().ToString().Substring(0, 13);

            var manifestName = liveEventName.ToLower();

            if (data.archiveWindowLength != null)
                eventInfoFromCosmos.ArchiveWindowLength = (int)data.archiveWindowLength;

            var cencKey = new StreamingLocatorContentKey();
            var cbcsKey = new StreamingLocatorContentKey();

            if (!deleteAsset) // we need to regenerate the keys if the user wants to keep the asset as keys cannot be reused for more than one asset
            {
                try
                {
                    ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddEnvironmentVariables()
                            .Build(),
                            null);

                    MediaServicesHelpers.LogInformation(log, "Irdeto call...");

                    cencKey = await IrdetoHelpers.GenerateAndRegisterCENCKeyToIrdeto(liveEventName, config);
                    cbcsKey = await IrdetoHelpers.GenerateAndRegisterCBCSKeyToIrdeto(liveEventName, config);

                    MediaServicesHelpers.LogInformation(log, "Irdeto call done.");
                }
                catch (Exception ex)
                {
                    return IrdetoHelpers.ReturnErrorException(log, ex, "Irdeto response error");
                }
            }

            var clientTasks = new List<Task<LiveEventEntry>>();

            // list of locators guid for the new locators
            var locatorGuids = new List<Guid>();
            for (int i = 0; i < 10; i++)
            {
                locatorGuids.Add(Guid.NewGuid());
            }

            foreach (var region in azureRegions)
            {
                var task = Task<LiveEventEntry>.Run(async () =>
                {
                    Asset asset = null;
                    LiveEvent liveEvent = null;
                    LiveOutput liveOutput = null;

                    bool reuseKeys = false;
                    string storageAccountName = null;
                    var streamingLocatorsPolicies = new Dictionary<string, string>(); // locator name, policy name 


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
                    liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                    if (liveEvent == null)
                        throw new Exception("Error : live event does not exist !");

                    if (liveEvent.ResourceState != LiveEventResourceState.Running && liveEvent.ResourceState != LiveEventResourceState.Stopped)
                        throw new Exception("Error : live event should be in Running or Stopped state !");

                    // get live output(s) - it should be one
                    var myLiveOutputs = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEventName);

                    // get the names of the streaming policies. If not possible, recreate it
                    if (myLiveOutputs.FirstOrDefault() != null)
                    {
                        asset = client.Assets.Get(config.ResourceGroup, config.AccountName,
                            myLiveOutputs.First().AssetName);

                        var streamingLocatorsNames = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, asset.Name).StreamingLocators.Select(l => l.Name);
                        foreach (var locatorName in streamingLocatorsNames)
                        {
                            var locator =
                                client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, locatorName);
                            if (locator != null)
                            {
                                streamingLocatorsPolicies.Add(locatorName, locator.StreamingPolicyName);
                                if (locator.StreamingPolicyName != PredefinedStreamingPolicy.ClearStreamingOnly) // let's backup the keys to reuse them
                                {
                                    if (deleteAsset) // we reuse the keys. Only possible if the previous asset is deleted
                                    {
                                        reuseKeys = true;
                                        var keys = client.StreamingLocators.ListContentKeys(config.ResourceGroup, config.AccountName, locatorName).ContentKeys;
                                        cencKey = keys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCenc).FirstOrDefault();
                                        cbcsKey = keys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCbcs).FirstOrDefault();
                                    }
                                }
                            }
                        }

                        if (streamingLocatorsPolicies.Count == 0) // no way to get the streaming policy, let's read Cosmos or create a new one
                        {
                            MediaServicesHelpers.LogInformation(log, "Trying to read streaming policy from Cosmos.", region);
                            string streamingPolicyName = null;
                            // Load streaming policy info from Cosmos
                            try
                            {
                                var info = await CosmosHelpers.ReadStreamingPolicyDocument(new StreamingPolicyInfo(false)
                                {
                                    AMSAccountName = config.AccountName
                                });

                                if (info == null)
                                {
                                    log.LogWarning("Streaming policy not read from Cosmos.");
                                }
                                else
                                {
                                    streamingPolicyName = info.StreamingPolicyName;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Error reading Cosmos DB", ex);
                            }


                            // STREAMING POLICY CREATION
                            if (streamingPolicyName == null) // not found in Cosmos let's create a new one
                            {
                                StreamingPolicy streamingPolicy;
                                MediaServicesHelpers.LogInformation(log, "Creating streaming policy.", region);
                                try
                                {
                                    streamingPolicy = await IrdetoHelpers.CreateStreamingPolicyIrdeto(config, client, uniquenessPolicyName);
                                    streamingLocatorsPolicies.Add("", streamingPolicy.Name);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Streaming policy creation error", ex);
                                }

                                try
                                {
                                    if (!await CosmosHelpers.CreateOrUpdatePolicyDocument(new StreamingPolicyInfo(false)
                                    {
                                        AMSAccountName = config.AccountName,
                                        StreamingPolicyName = streamingPolicy.Name
                                    }))
                                    {
                                        log.LogWarning("Cosmos access not configured or error.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Streaming policy write error to Cosmos", ex);
                                }
                            }
                            else
                            {
                                streamingLocatorsPolicies.Add("", streamingPolicyName);

                            }
                        }
                    }

                    // let's purge all live output for now
                    foreach (var p in myLiveOutputs)
                    {
                        var assetName = p.AssetName;

                        var thisAsset = client.Assets.Get(config.ResourceGroup, config.AccountName, p.AssetName);
                        if (thisAsset != null)
                            storageAccountName = thisAsset.StorageAccountName; // let's backup storage account name to create the new asset here
                        MediaServicesHelpers.LogInformation(log, "deleting live output : " + p.Name, region);
                        await client.LiveOutputs.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name, p.Name);
                        if (deleteAsset)
                        {
                            MediaServicesHelpers.LogInformation(log, "deleting asset : " + assetName, region);
                            await client.Assets.DeleteAsync(config.ResourceGroup, config.AccountName, assetName);
                        }
                    }

                    if (liveEvent.ResourceState == LiveEventResourceState.Running)
                    {
                        MediaServicesHelpers.LogInformation(log, "reseting live event : " + liveEvent.Name, region);
                        await client.LiveEvents.ResetAsync(config.ResourceGroup, config.AccountName, liveEvent.Name);
                    }
                    else
                    {
                        MediaServicesHelpers.LogInformation(log, "Skipping the reset of live event " + liveEvent.Name + " as it is stopped.", region);
                    }

                    // LIVE OUTPUT CREATION
                    MediaServicesHelpers.LogInformation(log, "Live output creation...", region);

                    try
                    {
                        MediaServicesHelpers.LogInformation(log, "Asset creation...", region);
                        asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName,
                            "asset-" + uniquenessAssets, new Asset(storageAccountName: storageAccountName));

                        Hls hlsParam = null;
                        liveOutput = new LiveOutput(asset.Name, TimeSpan.FromMinutes(eventInfoFromCosmos.ArchiveWindowLength),
                            null, "output-" + uniquenessAssets, null, null, manifestName,
                            hlsParam);

                        MediaServicesHelpers.LogInformation(log, "create live output...", region);
                        await client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName, liveOutput.Name, liveOutput);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("live output creation error", ex);
                    }

                    try
                    {
                        // streaming locator(s) creation
                        MediaServicesHelpers.LogInformation(log, "Locator creation...", region);
                        int i = 0;
                        foreach (var entryLocPol in streamingLocatorsPolicies)
                        {
                            StreamingLocator locator = null;
                            var streamingLocatorGuid = locatorGuids[i]; // same locator for the two ouputs if 2 live event namle created
                            var uniquenessLocator = streamingLocatorGuid.ToString().Substring(0, 13);
                            var streamingLocatorName = "locator-" + uniquenessLocator;

                            if (entryLocPol.Value == PredefinedStreamingPolicy.ClearStreamingOnly)
                            {
                                locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset, streamingLocatorGuid);
                            }
                            else // DRM content
                            {
                                MediaServicesHelpers.LogInformation(log, "creating DRM locator : " + streamingLocatorName, region);

                                if (!reuseKeys)
                                {
                                    MediaServicesHelpers.LogInformation(log, "using new keys.", region);

                                    locator = await IrdetoHelpers.SetupDRMAndCreateLocatorWithNewKeys(
                                       config, entryLocPol.Value, streamingLocatorName, client, asset, cencKey, cbcsKey, streamingLocatorGuid);

                                }
                                else // no need to create new keys, let's use the exiting one
                                {
                                    MediaServicesHelpers.LogInformation(log, "using existing keys.", region);

                                    locator = await IrdetoHelpers.SetupDRMAndCreateLocatorWithExistingKeys(
                                       config, entryLocPol.Value, streamingLocatorName, client, asset, cencKey, cbcsKey, streamingLocatorGuid);

                                }
                            }
                            MediaServicesHelpers.LogInformation(log, "locator : " + streamingLocatorName, region);
                            i++;
                        }

                        await client.Assets.UpdateAsync(config.ResourceGroup, config.AccountName, asset.Name, asset);

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("locator creation error", ex);
                    }

                    var generalOutputInfo =
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