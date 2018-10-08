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
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.Media;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using LiveDrmOperationsV3.Models;
using LiveDrmOperationsV3.Helpers;

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
                var helper = new CosmosHelpers(log, config);
                generalOutputInfo.LiveEvents.ForEach(async l => await helper.CreateOrUpdateGeneralInfoDocument(l));
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