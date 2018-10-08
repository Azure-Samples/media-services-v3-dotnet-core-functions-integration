//
// Azure Media Services REST API v3 Functions
//
// create-live-event-output - This function creates a new live event and output (to be used with Irdeto)
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
    "LiveEvents": [
        {
            "Name": "SRF1",
            "ResourceState": "Running",
            "vanityUrl": false,
            "Input": [
                {
                    "Protocol": "FragmentedMP4",
                    "Url": "http://3b83aa39a3e2433e9a15a0c20c31bcaa.channel.media.azure.net/65c3026e74734c2d9a8dce4a1ed8f95e/ingest.isml"
                }
            ],
            "Preview": [
                {
                    "Protocol": "FragmentedMP4",
                    "Url": "https://srf1-srgssrliveeuno.preview-euno.channel.media.azure.net/75a4a8e8-d764-4290-965e-f831c8e04b97/preview.ism/manifest"
                }
            ],
            "LiveOutputs": [
                {
                    "Name": "output-06d00dff-c281",
                    "ArchiveWindowLength": "00:10:00",
                    "ResourceState": "Running",
                    "StreamingLocatorName": "locator-06d00dff-c281",
                    "StreamingPolicyName": "SRF1-bca5d6d7-da3a",
                    "Drm": [
                        {
                            "Type": "FairPlay",
                            "LicenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/SRG/getckc?ContentId=SRF1&KeyId=c7175624-0fb7-4ba5-a0e1-427cafcb456d"
                        },
                        {
                            "Type": "PlayReady",
                            "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/SRG/license?ContentId=SRF1"
                        },
                        {
                            "Type": "Widevine",
                            "LicenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/SRG/license&ContentId=SRF1"
                        }
                    ],
                    "CencKeyId": "c9d2dae5-ba14-46ba-aa2c-7ec2214bedd9",
                    "CbcsKeyId": "c7175624-0fb7-4ba5-a0e1-427cafcb456d",
                    "Urls": [
                        {
                            "Url": "https://srgssrliveeuno-euno.streaming.media.azure.net/167ed1d5-a2b2-42e9-92cf-7aaa53b738cf/srf1.ism/manifest(encryption=cenc)",
                            "Protocol": "SmoothStreaming"
                        },
                        {
                            "Url": "https://srgssrliveeuno-euno.streaming.media.azure.net/167ed1d5-a2b2-42e9-92cf-7aaa53b738cf/srf1.ism/manifest(format=mpd-time-csf,encryption=cenc)",
                            "Protocol": "DashCsf"
                        },
                        {
                            "Url": "https://srgssrliveeuno-euno.streaming.media.azure.net/167ed1d5-a2b2-42e9-92cf-7aaa53b738cf/srf1.ism/manifest(format=mpd-time-cmaf,encryption=cenc)",
                            "Protocol": "DashCmaf"
                        },
                        {
                            "Url": "https://srgssrliveeuno-euno.streaming.media.azure.net/167ed1d5-a2b2-42e9-92cf-7aaa53b738cf/srf1.ism/manifest(format=m3u8-cmaf,encryption=cbcs-aapl)",
                            "Protocol": "HlsCmaf"
                        },
                        {
                            "Url": "https://srgssrliveeuno-euno.streaming.media.azure.net/167ed1d5-a2b2-42e9-92cf-7aaa53b738cf/srf1.ism/manifest(format=m3u8-aapl,encryption=cbcs-aapl)",
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
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json.Linq;
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