//
// Azure Media Services REST API v3 Functions
//
// generate-resource -  This function generate a json structure to be used by the customer in their backend
//
/*
```c#
Input :
{
    "assetMain": <json structure>,
    "assetSub": <json structure>,
    "cdnHostName": "customertest-euwe.akamaized.net"
}


Output:
<json structure>
```
*/

using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class generateresource
    {
        [FunctionName("generate-resource")]
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

            AssetEntry assetEntry1 = null;

            if (data.assetMain != null)
            {
                assetEntry1 = ((JObject)data?.assetMain).ToObject<AssetEntry>();
            }

            AssetEntry assetEntry2 = null;
            if (data.assetSub != null)
            {
                assetEntry2 = ((JObject)data?.assetSub).ToObject<AssetEntry>();
            }

            string newHostName = (string)data.cdnHostName;
            if (string.IsNullOrWhiteSpace(newHostName)) newHostName = null;

            VodResource vodResource = new VodResource()
            {
                Urn = assetEntry1.Urn,
                ResourceList = new List<ResourceList>(),
                MainAsset = assetEntry1
            };


            var subtitlesList = new List<Subtitles>();
            if (assetEntry2 != null)
            {
                var locator2 = assetEntry2.StreamingLocators.FirstOrDefault();
                var subtitlesUrl = locator2?.Urls.Where(p => p.Protocol == "Download");
                var subtitlesSema = assetEntry2?.Semaphore.Files.Where(f => f.CopyToSubAsset);

                var query = from subInfo in subtitlesSema
                            join sub in subtitlesUrl on subInfo.FileName equals (new Uri(sub.Url)).Segments[(new Uri(sub.Url)).Segments.Length - 1]
                            select new Subtitles { Url = MediaServicesHelpers.UpdateHostNameIfNeeded(newHostName, sub.Url), TextLanguage = subInfo.TextLanguage, TextTitle = subInfo.TextTitle };
                subtitlesList = query.ToList();
            }

            var locator1 = assetEntry1.StreamingLocators.FirstOrDefault();

            var resDashCsf = new ResourceList()
            {
                Url = MediaServicesHelpers.UpdateHostNameIfNeeded(newHostName, locator1?.Urls.Where(u => u.Protocol == "DashCsf").FirstOrDefault()?.Url),
                Protocol = "DASH",
                MimeType = "application/dash+xml",
                VideoCodec = "H264",
                AudioCodec = "AAC",
                MediaContainer = "MP4",
                Quality = "SD",
                Live = false,
                DrmList = new List<DrmList>() {
                        new DrmList(){ Type = "PlayReady", LicenseUrl =locator1?.Drm?.Where(d=> d.Type=="PlayReady").FirstOrDefault()?.LicenseUrl },
                        new DrmList(){ Type = "Widevine", LicenseUrl =locator1?.Drm?.Where(d=> d.Type=="Widevine").FirstOrDefault()?.LicenseUrl }
                    },
                SubTitles = subtitlesList
            };

            vodResource.ResourceList.Add(resDashCsf);

            var resHlsTs = new ResourceList()
            {
                Url = MediaServicesHelpers.UpdateHostNameIfNeeded(newHostName, locator1?.Urls.Where(u => u.Protocol == "HlsTs").FirstOrDefault()?.Url),
                Protocol = "HLS",
                MimeType = "application/x-mpegURL",
                VideoCodec = "H264",
                AudioCodec = "AAC",
                MediaContainer = "MP4",
                Quality = "SD",
                Live = false,
                DrmList = new List<DrmList>() {
                        new DrmList(){
                            Type = "FairPlay",
                            LicenseUrl =locator1.Drm.Where(d=> d.Type=="FairPlay").FirstOrDefault().LicenseUrl,
                            CertificateUrl = locator1.Drm.Where(d=> d.Type=="FairPlay").FirstOrDefault().CertificateUrl
                        },
                     },
                SubTitles = subtitlesList
            };
            vodResource.ResourceList.Add(resHlsTs);


            // let's write it to Cosmos
            if (!await CosmosHelpers.CreateOrUpdateVODResourceDocument(vodResource))
                log.LogWarning("Cosmos access not configured.");


            return new OkObjectResult(
                                        JsonConvert.SerializeObject(vodResource, Formatting.Indented)
                                    );
        }
    }
}