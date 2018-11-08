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
    "liveEventAutoStart": False,  // optional. Default is True
    "azureRegion": "euwe" or "we" or "euno" or "no", // optional. If this value is set, then the AMS account name and resource group are appended with this value. Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty. This feature is useful if you want to manage several AMS account in different regions. Note: the service principal must work with all this accounts
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
    public static class CreateChannel
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("create-live-event-output")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            ConfigWrapper config;

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

            // init default

            StreamingPolicy streamingPolicy = null;
            var uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            var streamingLocatorName = "locator-" + uniqueness;
            var manifestName = liveEventName.ToLower();

            var useDRM = data.useDRM != null ? (bool)data.useDRM : true;
            Asset asset = null;
            LiveEvent liveEvent = null;
            LiveOutput liveOutput = null;


            if (data.archiveWindowLength != null)
                eventInfoFromCosmos.ArchiveWindowLength = (int)data.archiveWindowLength;

            if (eventInfoFromCosmos.BaseStorageName != null)
                eventInfoFromCosmos.StorageName = eventInfoFromCosmos.BaseStorageName + config.AzureRegionCode;

            if (data.storageAccountName != null) eventInfoFromCosmos.StorageName = (string)data.storageAccountName;

            if (data.inputProtocol != null && ((string)data.inputProtocol).ToUpper() == "RTMP")
                eventInfoFromCosmos.InputProtocol = LiveEventInputProtocol.RTMP;

            if (data.liveEventAutoStart != null) eventInfoFromCosmos.AutoStart = (bool)data.liveEventAutoStart;

            if (data.InputACL != null) eventInfoFromCosmos.LiveEventInputACL = (List<string>)data.InputACL;

            if (data.PreviewACL != null) eventInfoFromCosmos.LiveEventPreviewACL = (List<string>)data.PreviewACL;

            if (data.lowLatency != null) eventInfoFromCosmos.LowLatency = (bool)data.lowLatency;

            var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;


            // LIVE EVENT CREATION
            log.LogInformation("Live event creation...");

            try
            {
                // let's check that the channel does not exist already
                liveEvent = await client.LiveEvents.GetAsync(config.ResourceGroup, config.AccountName, liveEventName);
                if (liveEvent != null)
                    return IrdetoHelpers.ReturnErrorException(log, "Error : live event already exists !");

                // IP ACL for preview URL
                //var ipAclListPreview = config.LiveEventPreviewACL?.Trim().Split(';').ToList();
                var ipsPreview = new List<IPRange>();
                if (eventInfoFromCosmos.LiveEventPreviewACL == null ||
                    eventInfoFromCosmos.LiveEventPreviewACL.Count == 0)
                {
                    log.LogInformation("preview all");
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
                //var ipAclListInput = config.LiveEventInputACL?.Trim().Split(';').ToList();
                var ipsInput = new List<IPRange>();

                //  if (config.LiveEventInputACL == null || config.LiveEventInputACL.Trim() == "" || ipAclListInput == null || ipAclListInput.Count == 0)
                if (eventInfoFromCosmos.LiveEventInputACL == null || eventInfoFromCosmos.LiveEventInputACL.Count == 0)
                {
                    log.LogInformation("input all");
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
                log.LogInformation("Live event created.");
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "live event creation error");
            }

            if (useDRM)
            {
                // STREAMING POLICY CREATION
                log.LogInformation("Creating streaming policy.");
                try
                {
                    streamingPolicy = await IrdetoHelpers.CreateStreamingPolicyIrdeto(liveEventName, config, client);
                }
                catch (Exception ex)
                {
                    return IrdetoHelpers.ReturnErrorException(log, ex, "streaming policy creation error");
                }
            }

            // LIVE OUTPUT CREATION
            log.LogInformation("Live output creation...");

            try
            {
                log.LogInformation("Asset creation...");

                asset = await client.Assets.CreateOrUpdateAsync(config.ResourceGroup, config.AccountName,
                    "asset-" + uniqueness,
                    new Asset(storageAccountName: eventInfoFromCosmos.StorageName));

                Hls hlsParam = null;

                liveOutput = new LiveOutput(asset.Name, TimeSpan.FromMinutes(eventInfoFromCosmos.ArchiveWindowLength),
                    null, "output-" + uniqueness, null, null, manifestName,
                    hlsParam); //we put the streaming locator in description
                log.LogInformation("await task...");

                log.LogInformation("create live output...");
                await client.LiveOutputs.CreateAsync(config.ResourceGroup, config.AccountName, liveEventName,
                    liveOutput.Name, liveOutput);
                log.LogInformation("Asset created.");
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex, "live output creation error");
            }

            string cenckeyId = null;
            string cenccontentKey = null;
            string cbcskeyId = null;
            string cbcscontentKey = null;

            if (useDRM)
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
                    log.LogInformation("Irdeto call done.");
                }
                catch (Exception ex)
                {
                    return IrdetoHelpers.ReturnErrorException(log, ex, "Irdeto response error");
                }


            try
            {
                // let's get the asset
                // in v3, asset name = asset if in v2 (without prefix)
                log.LogInformation("Asset configuration.");

                StreamingLocator locator = null;
                if (useDRM)
                {
                    locator = await IrdetoHelpers.SetupDRMAndCreateLocatorWithNewKeys(config, streamingPolicy.Name,
                        streamingLocatorName, client, asset, cenckeyId, cenccontentKey, cbcskeyId, cbcscontentKey);
                }
                else // no DRM
                {
                    locator = new StreamingLocator(asset.Name, null);
                    locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName,
                        streamingLocatorName, locator);
                }

                log.LogInformation("locator : " + locator.Name);

                // await taskLiveOutputCreation;
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