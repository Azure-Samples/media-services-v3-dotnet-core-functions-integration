//
// Azure Media Services REST API v3 Functions
//
// reset-live-event-output - This function resets a new live event and output (to be used with Irdeto)
//
/*
```c#
Input :
{
    "liveEventName": "CH1",
    "deleteAsset" : false, // optional, default is True
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Usefull if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
    "archiveWindowLength" : 20  // value in minutes, optional. Default is 10 (minutes)
}


Output:
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
                    data.azureRegion != null ? (string) data.azureRegion : null
                );
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            log.LogInformation("config loaded.");

            var liveEventName = (string) data.liveEventName;
            if (liveEventName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass liveEventName in the JSON");

            // default settings
            var eventInfoFromCosmos = new LiveEventSettingsInfo
            {
                liveEventName = liveEventName
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


            // init default

            var deleteAsset = true;
            if (data.deleteAsset != null) deleteAsset = (bool) data.deleteAsset;

            var uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            var manifestName = liveEventName.ToLower();


            var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            Asset asset = null;
            LiveEvent liveEvent = null;
            LiveOutput liveOutput = null;
            Task<LiveOutput> taskLiveOutputCreation = null;

            var streamingLocatorsPolicies = new Dictionary<string, string>(); // locator name, policy name 
            string storageAccountName = null;

            Task taskReset = null;

            if (data.archiveWindowLength != null)
                eventInfoFromCosmos.archiveWindowLength = (int) data.archiveWindowLength;

            try
            {
                // let's check that the channel exists
                liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                if (liveEvent == null)
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event does not exist !");

                if (liveEvent.ResourceState != LiveEventResourceState.Running)
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event is not running !");

                // get live output(s) - it should be one
                var myLiveOutputs = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEventName);


                // get the names of the streaming policies. If not possible, recreate it
                if (myLiveOutputs.First() != null)
                {
                    asset = client.Assets.Get(config.ResourceGroup, config.AccountName,
                        myLiveOutputs.First().AssetName);

                    var streamingLocatorNames = IrdetoHelpers.ReturnLocatorNameFromDescription(asset);
                    foreach (var locatorName in streamingLocatorNames)
                    {
                        var locator =
                            client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, locatorName);
                        if (locator != null) streamingLocatorsPolicies.Add(locatorName, locator.StreamingPolicyName);
                    }
                }

                if (streamingLocatorsPolicies.Count == 0) // no way to get the streaming policy, let's create a new one
                {
                    log.LogInformation("Creating streaming policy.");
                    var streamingPolicy =
                        await IrdetoHelpers.CreateStreamingPolicyIrdeto(liveEventName, config, client);
                    streamingLocatorsPolicies.Add("", streamingPolicy.Name);
                }

                // let's purge all live output for now
                foreach (var p in myLiveOutputs)
                {
                    var assetName = p.AssetName;

                    var thisAsset = client.Assets.Get(config.ResourceGroup, config.AccountName, p.AssetName);
                    if (thisAsset != null)
                        storageAccountName =
                            thisAsset
                                .StorageAccountName; // let's backup storage account name to create the new asset here
                    log.LogInformation("deleting live output : " + p.Name);
                    await client.LiveOutputs.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name,
                        p.Name);
                    if (deleteAsset)
                    {
                        log.LogInformation("deleting asset : " + assetName);
                        client.Assets.DeleteAsync(config.ResourceGroup, config.AccountName, assetName);
                    }
                }

                if (liveEvent.ResourceState == LiveEventResourceState.Running)
                {
                    log.LogInformation("reseting live event : " + liveEvent.Name);
                    taskReset = client.LiveEvents.ResetAsync(config.ResourceGroup, config.AccountName, liveEvent.Name);
                }
                else
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event is not running !");
                }
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }


            // LIVE OUTPUT CREATION
            log.LogInformation("Live output creation...");

            try
            {
                log.LogInformation("Asset creation...");
                asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName,
                    "asset-" + uniqueness, new Asset(storageAccountName: storageAccountName, description: null));

                Hls hlsParam = null;
                liveOutput = new LiveOutput(asset.Name, TimeSpan.FromMinutes(eventInfoFromCosmos.archiveWindowLength),
                    null, "output-" + uniqueness, null, null, manifestName,
                    hlsParam); //we put the streaming locator in description
                log.LogInformation("await task reset...");

                await taskReset; // let's wait for the reset to complete

                log.LogInformation("create live output...");
                taskLiveOutputCreation = client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName,
                    liveEventName, liveOutput.Name, liveOutput);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "live output creation error");
            }

            string cenckeyId;
            string cenccontentKey;
            string cbcskeyId;
            string cbcscontentKey;

            try
            {
                log.LogInformation("Irdeto call...");

                // CENC Key
                var cencGuid = Guid.NewGuid();
                cenckeyId = cencGuid.ToString();
                cenccontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
                var cencIV = Convert.ToBase64String(cencGuid.ToByteArray());
                var responsecenc = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService,
                    liveEventName, config, cenckeyId, cenccontentKey, cencIV, false);
                var contentcenc = await responsecenc.Content.ReadAsStringAsync();

                if (responsecenc.StatusCode != HttpStatusCode.OK)
                    return IrdetoHelpers.ReturnErrorException(log, "Error Irdeto response cenc - " + contentcenc);

                var cenckeyIdFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcenc, "KeyId=");
                var cenccontentKeyFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcenc, "ContentKey=");

                if (cenckeyId != cenckeyIdFromIrdeto || cenccontentKey != cenccontentKeyFromIrdeto)
                    return IrdetoHelpers.ReturnErrorException(log, "Error CENC not same key - " + contentcenc);

                // CBCS Key
                var cbcsGuid = Guid.NewGuid();
                cbcskeyId = cbcsGuid.ToString();
                cbcscontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
                var cbcsIV =
                    Convert.ToBase64String(
                        IrdetoHelpers.HexStringToByteArray(cbcsGuid.ToString().Replace("-", string.Empty)));
                var responsecbcs = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService,
                    liveEventName, config, cbcskeyId, cbcscontentKey, cbcsIV, true);
                var contentcbcs = await responsecbcs.Content.ReadAsStringAsync();

                if (responsecbcs.StatusCode != HttpStatusCode.OK)
                    return IrdetoHelpers.ReturnErrorException(log, "Error Irdeto response cbcs - " + contentcbcs);

                var cbcskeyIdFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "KeyId=");
                var cbcscontentKeyFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "ContentKey=");

                if (cbcskeyId != cbcskeyIdFromIrdeto || cbcscontentKey != cbcscontentKeyFromIrdeto)
                    return IrdetoHelpers.ReturnErrorException(log, "Error CBCS not same key - " + contentcbcs);
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "Irdeto response error");
            }

            try
            {
                // streaming locator(s) creation
                log.LogInformation("Locator creation...");

                foreach (var entryLocPol in streamingLocatorsPolicies)
                {
                    string streamingLocatorName;

                    if (entryLocPol.Value == PredefinedStreamingPolicy.ClearStreamingOnly)
                    {
                        var uniqueness2 = Guid.NewGuid().ToString().Substring(0, 13);
                        streamingLocatorName = "locator-" + uniqueness2; // another uniqueness
                        log.LogInformation("creating clear locator : " + streamingLocatorName);
                        var locator =
                            await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset);
                    }
                    else // DRM content
                    {
                        streamingLocatorName = "locator-" + uniqueness; // sae uniqueness that asset or output
                        log.LogInformation("creating DRM locator : " + streamingLocatorName);
                        var locator = await IrdetoHelpers.SetupDRMAndCreateLocator(config, entryLocPol.Value,
                            streamingLocatorName, client, asset, cenckeyId, cenccontentKey, cbcskeyId, cbcscontentKey);
                    }

                    log.LogInformation("locator : " + streamingLocatorName);
                    asset.Description =
                        IrdetoHelpers.SetLocatorNameInDescription(streamingLocatorName, asset.Description);
                }

                await client.Assets.UpdateAsync(config.ResourceGroup, config.AccountName, asset.Name, asset);

                await taskLiveOutputCreation;
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
                generalOutputInfo = GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent>
                {
                    liveEvent
                });
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