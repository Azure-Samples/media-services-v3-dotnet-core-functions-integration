//
// Azure Media Services REST API v3 Functions
//
// create-live-event-output - This function creates a new live event and output (to be used with Irdeto)
//
/*
```c#
Input :
{
    "liveEventName": "CH1",
    "storageAccountName" : "", // optional. Specify in which attached storage account the asset should be created. If azureRegion is specified, then the region is appended to the name
    "inputProtocol" : "FragmentedMP4" or "RTMP",  // value is optional. Default is FragmentedMP4
    "vanityUrl" : true, // VanityUrl if true then LiveEvent has a predictable ingest URL even when stopped. It takes more time to get it. Non Vanity URL Live Event are quicker to get, but ingest is only known when the live event is running
    "archiveWindowLength" : 20,  // value in minutes, optional. Default is 10 (minutes)
    "liveEventAutoStart": false,  // optional. Default is True
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, two live events will be created.
            // Note: the service principal must work with all this accounts
    "useDRM" : true, // optional. Default is true. Specify false if you don't want to use dynamic encryption
    "lowLatency" : false, // optional. Set to true for low latency
    "InputACL": [  // optional
                "192.168.0.1/24"
            ],
    "PreviewACL": [ // optional
                "192.168.0.0/24"
            ],
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
            }
          ]
        }
      ]
    }
  ]
}
```
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public static class CreateChannel
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("create-live-event-output")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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
            var eventInfoFromCosmos = new LiveEventSettingsInfo()
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
            var uniquenessAssets = Guid.NewGuid().ToString().Substring(0, 13);

            var streamingLocatorGuid = Guid.NewGuid(); // same locator for the two ouputs if 2 live event namle created
            var uniquenessLocator = streamingLocatorGuid.ToString().Substring(0, 13);
            var streamingLocatorName = "locator-" + uniquenessLocator;

            string uniquenessPolicyName = Guid.NewGuid().ToString().Substring(0, 13);

            var manifestName = liveEventName.ToLower();

            var useDRM = data.useDRM != null ? (bool)data.useDRM : true;

            if (data.archiveWindowLength != null)
                eventInfoFromCosmos.ArchiveWindowLength = (int)data.archiveWindowLength;

            if (data.inputProtocol != null && ((string)data.inputProtocol).ToUpper() == "RTMP")
                eventInfoFromCosmos.InputProtocol = LiveEventInputProtocol.RTMP;

            if (data.liveEventAutoStart != null) eventInfoFromCosmos.AutoStart = (bool)data.liveEventAutoStart;

            if (data.InputACL != null) eventInfoFromCosmos.LiveEventInputACL = (List<string>)data.InputACL;

            if (data.PreviewACL != null) eventInfoFromCosmos.LiveEventPreviewACL = (List<string>)data.PreviewACL;

            if (data.lowLatency != null) eventInfoFromCosmos.LowLatency = (bool)data.lowLatency;

            var cencKey = new StreamingLocatorContentKey();
            var cbcsKey = new StreamingLocatorContentKey();

            if (useDRM)
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

            foreach (var region in azureRegions)
            {
                var task = Task<LiveEventEntry>.Run(async () =>
                {
                    Asset asset = null;
                    LiveEvent liveEvent = null;
                    LiveOutput liveOutput = null;
                    StreamingPolicy streamingPolicy = null;
                    string storageName = null;

                    ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddEnvironmentVariables()
                            .Build(),
                            region
                    );

                    MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                    MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                    if (eventInfoFromCosmos.BaseStorageName != null)
                        storageName = eventInfoFromCosmos.BaseStorageName + config.AzureRegionCode;

                    if (data.storageAccountName != null) storageName = (string)data.storageAccountName;

                    var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                    // Set the polling interval for long running operations to 2 seconds.
                    // The default value is 30 seconds for the .NET client SDK
                    client.LongRunningOperationRetryTimeout = 2;

                    // LIVE EVENT CREATION
                    MediaServicesHelpers.LogInformation(log, "Live event creation...", region);

                    // let's check that the channel does not exist already
                    liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                    if (liveEvent != null)
                        throw new Exception("Error : live event already exists !");

                    // IP ACL for preview URL
                    var ipsPreview = new List<IPRange>();
                    if (eventInfoFromCosmos.LiveEventPreviewACL == null ||
                        eventInfoFromCosmos.LiveEventPreviewACL.Count == 0)
                    {
                        MediaServicesHelpers.LogInformation(log, "preview all", region);
                        var ip = new IPRange
                        { Name = "AllowAll", Address = IPAddress.Parse("0.0.0.0").ToString(), SubnetPrefixLength = 0 };
                        ipsPreview.Add(ip);
                    }
                    else
                    {
                        foreach (var ipacl in eventInfoFromCosmos.LiveEventPreviewACL)
                        {
                            var ipaclcomp = ipacl.Split('/'); // notation can be "192.168.0.1" or "192.168.0.1/32"
                            var subnet = ipaclcomp.Count() > 1 ? Convert.ToInt32(ipaclcomp[1]) : 0;
                            var ip = new IPRange
                            {
                                Name = "ip",
                                Address = IPAddress.Parse(ipaclcomp[0]).ToString(),
                                SubnetPrefixLength = subnet
                            };
                            ipsPreview.Add(ip);
                        }
                    }

                    var liveEventPreview = new LiveEventPreview
                    {
                        AccessControl = new LiveEventPreviewAccessControl(new IPAccessControl(ipsPreview))
                    };

                    // IP ACL for input URL
                    var ipsInput = new List<IPRange>();

                    if (eventInfoFromCosmos.LiveEventInputACL == null || eventInfoFromCosmos.LiveEventInputACL.Count == 0)
                    {
                        MediaServicesHelpers.LogInformation(log, "input all", region);
                        var ip = new IPRange
                        { Name = "AllowAll", Address = IPAddress.Parse("0.0.0.0").ToString(), SubnetPrefixLength = 0 };
                        ipsInput.Add(ip);
                    }
                    else
                    {
                        foreach (var ipacl in eventInfoFromCosmos.LiveEventInputACL)
                        {
                            var ipaclcomp = ipacl.Split('/'); // notation can be "192.168.0.1" or "192.168.0.1/32"
                            var subnet = ipaclcomp.Count() > 1 ? Convert.ToInt32(ipaclcomp[1]) : 0;
                            var ip = new IPRange
                            {
                                Name = "ip",
                                Address = IPAddress.Parse(ipaclcomp[0]).ToString(),
                                SubnetPrefixLength = subnet
                            };
                            ipsInput.Add(ip);
                        }
                    }

                    var liveEventInput = new LiveEventInput(
                                                            eventInfoFromCosmos.InputProtocol,
                                                            accessControl: new LiveEventInputAccessControl(new IPAccessControl(ipsInput)),
                                                            accessToken: config.LiveIngestAccessToken
                        );

                    liveEvent = new LiveEvent(
                        name: liveEventName,
                        location: config.Region,
                        description: "",
                        vanityUrl: eventInfoFromCosmos.VanityUrl,
                        encoding: new LiveEventEncoding { EncodingType = LiveEventEncodingType.None },
                        input: liveEventInput,
                        preview: liveEventPreview,
                        streamOptions: new List<StreamOptionsFlag?>
                        {
                        // Set this to Default or Low Latency
                        eventInfoFromCosmos.LowLatency ?  StreamOptionsFlag.LowLatency : StreamOptionsFlag.Default
                        }
                    );


                    liveEvent = await client.LiveEvents.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName,
                        liveEvent, eventInfoFromCosmos.AutoStart);
                    MediaServicesHelpers.LogInformation(log, "Live event created.", region);


                    if (useDRM)
                    {
                        // STREAMING POLICY CREATION
                        MediaServicesHelpers.LogInformation(log, "Creating streaming policy.", region);
                        try
                        {
                            streamingPolicy = await IrdetoHelpers.CreateStreamingPolicyIrdeto(liveEventName, config, client, uniquenessPolicyName);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("streaming policy creation error", ex);
                        }
                    }

                    // LIVE OUTPUT CREATION
                    MediaServicesHelpers.LogInformation(log, "Live output creation...", region);

                    try
                    {
                        MediaServicesHelpers.LogInformation(log, "Asset creation...", region);

                        asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName,
                            "asset-" + uniquenessAssets,
                            new Asset(storageAccountName: storageName));

                        Hls hlsParam = null;

                        liveOutput = new LiveOutput(asset.Name, TimeSpan.FromMinutes(eventInfoFromCosmos.ArchiveWindowLength),
                            null, "output-" + uniquenessAssets, null, null, manifestName,
                            hlsParam); //we put the streaming locator in description
                        MediaServicesHelpers.LogInformation(log, "await task...", region);

                        MediaServicesHelpers.LogInformation(log, "create live output...", region);
                        await client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName,
                            liveOutput.Name, liveOutput);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("live output creation error", ex);
                    }


                    try
                    {
                        // let's get the asset
                        // in v3, asset name = asset if in v2 (without prefix)
                        MediaServicesHelpers.LogInformation(log, "Asset configuration.", region);

                        StreamingLocator locator = null;
                        if (useDRM)
                        {
                            locator = await IrdetoHelpers.SetupDRMAndCreateLocatorWithNewKeys(config, streamingPolicy.Name,
                                streamingLocatorName, client, asset, cencKey, cbcsKey, streamingLocatorGuid);
                        }
                        else // no DRM
                        {
                            locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset, streamingLocatorGuid);
                        }

                        MediaServicesHelpers.LogInformation(log, "locator : " + locator.Name, region);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("locator creation error", ex);
                    }

                    // let's build info for the live event and output

                    var generalOutputInfo =
                        GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent> { liveEvent });

                    if (!await CosmosHelpers.CreateOrUpdateGeneralInfoDocument(generalOutputInfo.LiveEvents[0]))
                        log.LogWarning("Cosmos access not configured.");

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