//
// Azure Media Services REST API v3 Functions
//
// reset-live-event-output - This function resets a new live event and output (to be used with Irdeto)
//
/*
```c#
Input :
{
    "liveEventName": "FPOC",
    "deleteAsset" : false, // optional, default is True
    "azureRegion": "euwe" or "we" or "euno" or "no"// optional. If this value is set, then the AMS account name and resource group are appended with this value. Usefull if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
    "archiveWindowLength" : 20  // value in minutes, optional. Default is 10 (minutes)
}


Output:
{
    "Success": true,
    "LiveEvents": [
        {
            "Name": "TEST",
            "ResourceState": "Running",
            "vanityUrl": true,
            "Input": [
                {
                    "Protocol": "FragmentedMP4",
                    "Url": "http://test-prodliveeuwe-euwe.channel.media.azure.net/fe21a7147fb64498b52f024c41a3298e/ingest.isml"
                }
            ],
            "InputACL": [
                "192.168.0.1/24"
            ],
            "Preview": [
                {
                    "Protocol": "DashCsf",
                    "Url": "https://test-prodliveeuwe.preview-euwe.channel.media.azure.net/fbc40c48-07bd-4938-92f2-3375597d8ce3/preview.ism/manifest(format=mpd-time-csf)"
                }
            ],
            "PreviewACL": [
                "192.168.0.0/24"
            ],
            "LiveOutputs": [
                {
                    "Name": "output-8b49c322-8429",
                    "ArchiveWindowLength": 10,
                    "AssetName": "asset-8b49c322-8429",
                    "AssetStorageAccountName": "lsvdefaultdeveuwe",
                    "ResourceState": "Running",
                    "StreamingLocatorName": "locator-8b49c322-8429",
                    "StreamingPolicyName": "TEST-dd5a9c6b-b159",
                    "Drm": [
                        {
                            "Type": "FairPlay",
                            "LicenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/SRG/getckc?ContentId=SRF2&KeyId=dd5a9130-9734-45b4-945b-57516ee80945"
                        },
                        {
                            "Type": "PlayReady",
                            "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/CUSTOMER/license?ContentId=ID2"
                        },
                        {
                            "Type": "Widevine",
                            "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/CUSTOMER/license&ContentId=ID2"
                        }
                    ],
                    "CencKeyId": "3391a2a8-43e1-48e6-9d0b-39dd12a1d300",
                    "CbcsKeyId": "dd5a9130-9734-45b4-945b-57516ee80945",
                    "Urls": [
                        {
                            "Url": "https://prodliveeuwe-prodliveeuwe-euwe.streaming.media.azure.net/8d61c393-87dc-488b-a886-9adf9ba5bafc/test.ism/manifest(encryption=cenc)",
                            "Protocol": "SmoothStreaming"
                        },
                        {
                            "Url": "https://prodliveeuwe-prodliveeuwe-euwe.streaming.media.azure.net/8d61c393-87dc-488b-a886-9adf9ba5bafc/test.ism/manifest(format=mpd-time-csf,encryption=cenc)",
                            "Protocol": "DashCsf"
                        },
                        {
                            "Url": "https://prodliveeuwe-prodliveeuwe-euwe.streaming.media.azure.net/8d61c393-87dc-488b-a886-9adf9ba5bafc/test.ism/manifest(format=mpd-time-cmaf,encryption=cenc)",
                            "Protocol": "DashCmaf"
                        },
                        {
                            "Url": "https://prodliveeuwe-prodliveeuwe-euwe.streaming.media.azure.net/8d61c393-87dc-488b-a886-9adf9ba5bafc/test.ism/manifest(format=m3u8-cmaf,encryption=cbcs-aapl)",
                            "Protocol": "HlsCmaf"
                        },
                        {
                            "Url": "https://prodliveeuwe-prodliveeuwe-euwe.streaming.media.azure.net/8d61c393-87dc-488b-a886-9adf9ba5bafc/test.ism/manifest(format=m3u8-aapl,encryption=cbcs-aapl)",
                            "Protocol": "HlsTs"
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
using System.Net;
using System.Linq;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;

namespace LiveDrmOperationsV3
{
    public static class ResetChannel
    {
        private const string labelCenc = "cencDefaultKey";
        private const string labelCbcs = "cbcsDefaultKey";
        private const string assetprefix = "nb:cid:UUID:";

        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("reset-live-event-output")]
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

            bool deleteAsset = true;
            if (data.deleteAsset != null)
            {
                deleteAsset = (bool)data.deleteAsset;
            }

            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            string manifestName = liveEventName.ToLower();


            IAzureMediaServicesClient client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            Asset asset = null;
            LiveEvent liveEvent = null;
            LiveOutput liveOutput = null;
            Task<LiveOutput> taskLiveOutputCreation = null;

            Dictionary<string, string> streamingLocatorsPolicies = new Dictionary<string, string>(); // locator name, policy name 
            string storageAccountName = null;

            Task taskReset = null;

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

                // get live output(s) - it should be one
                var myLiveOutputs = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEventName);


                // get the names of the streaming policies. If not possible, recreate it
                if (myLiveOutputs.First() != null)
                {
                    asset = client.Assets.Get(config.ResourceGroup, config.AccountName, myLiveOutputs.First().AssetName);

                    var streamingLocatorNames = IrdetoHelpers.ReturnLocatorNameFromDescription(asset);
                    foreach (var locatorName in streamingLocatorNames)
                    {
                        var locator = client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, locatorName);
                        if (locator != null)
                        {
                            streamingLocatorsPolicies.Add(locatorName, locator.StreamingPolicyName);
                        }
                    }
                }

                if (streamingLocatorsPolicies.Count == 0) // no way to get the streaming policy, let's create a new one
                {
                    log.LogInformation("Creating streaming policy.");
                    var streamingPolicy = await IrdetoHelpers.CreateStreamingPolicyIrdeto(liveEventName, config, client);
                    streamingLocatorsPolicies.Add("", streamingPolicy.Name);
                }

                // let's purge all live output for now
                foreach (var p in myLiveOutputs)
                {
                    string assetName = p.AssetName;

                    var thisAsset = client.Assets.Get(config.ResourceGroup, config.AccountName, p.AssetName);
                    if (thisAsset != null)
                    {
                        storageAccountName = thisAsset.StorageAccountName; // let's backup storage account name to create the new asset here
                    }
                    log.LogInformation("deleting live output : " + p.Name);
                    await client.LiveOutputs.DeleteAsync(config.ResourceGroup, config.AccountName, liveEvent.Name, p.Name);
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
                asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName, "asset-" + uniqueness, new Asset(storageAccountName: storageAccountName, description: null));

                Hls hlsParam = null;
                liveOutput = new LiveOutput(asset.Name, TimeSpan.FromMinutes((double)eventInfoFromCosmos.archiveWindowLength), null, "output-" + uniqueness, null, null, manifestName, hlsParam); //we put the streaming locator in description
                log.LogInformation("await task reset...");

                await taskReset; // let's wait for the reset to complete

                log.LogInformation("create live output...");
                taskLiveOutputCreation = client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName, liveOutput.Name, liveOutput);
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
                Guid cencGuid = Guid.NewGuid();
                cenckeyId = cencGuid.ToString();
                cenccontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
                string cencIV = Convert.ToBase64String(cencGuid.ToByteArray());
                var responsecenc = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService, liveEventName, config, cenckeyId, cenccontentKey, cencIV, false);
                string contentcenc = await responsecenc.Content.ReadAsStringAsync();

                if (responsecenc.StatusCode != HttpStatusCode.OK)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error Irdeto response cenc - " + contentcenc);
                }

                string cenckeyIdFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcenc, "KeyId=");
                string cenccontentKeyFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcenc, "ContentKey=");

                if (cenckeyId != cenckeyIdFromIrdeto || cenccontentKey != cenccontentKeyFromIrdeto)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error CENC not same key - " + contentcenc);
                }

                // CBCS Key
                Guid cbcsGuid = Guid.NewGuid();
                cbcskeyId = cbcsGuid.ToString();
                cbcscontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
                string cbcsIV = Convert.ToBase64String(IrdetoHelpers.HexStringToByteArray(cbcsGuid.ToString().Replace("-", string.Empty)));
                var responsecbcs = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService, liveEventName, config, cbcskeyId, cbcscontentKey, cbcsIV, true);
                string contentcbcs = await responsecbcs.Content.ReadAsStringAsync();

                if (responsecbcs.StatusCode != HttpStatusCode.OK)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error Irdeto response cbcs - " + contentcbcs);
                }

                string cbcskeyIdFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "KeyId=");
                string cbcscontentKeyFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "ContentKey=");

                if (cbcskeyId != cbcskeyIdFromIrdeto || cbcscontentKey != cbcscontentKeyFromIrdeto)
                {
                    return IrdetoHelpers.ReturnErrorException(log, "Error CBCS not same key - " + contentcbcs);
                }

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
                        string uniqueness2 = Guid.NewGuid().ToString().Substring(0, 13);
                        streamingLocatorName = "locator-" + uniqueness2; // another uniqueness
                        log.LogInformation("creating clear locator : " + streamingLocatorName);
                        StreamingLocator locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset);

                    }
                    else // DRM content
                    {
                        streamingLocatorName = "locator-" + uniqueness; // sae uniqueness that asset or output
                        log.LogInformation("creating DRM locator : " + streamingLocatorName);
                        StreamingLocator locator = await IrdetoHelpers.SetupDRMAndCreateLocator(config, entryLocPol.Value, streamingLocatorName, client, asset, cenckeyId, cenccontentKey, cbcskeyId, cbcscontentKey);
                    }

                    log.LogInformation("locator : " + streamingLocatorName);
                    asset.Description = IrdetoHelpers.SetLocatorNameInDescription(streamingLocatorName, asset.Description);

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
                generalOutputInfo = GenerateInfoHelpers.GenerateOutputInformation(config, client, new List<LiveEvent>() { liveEvent
});
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