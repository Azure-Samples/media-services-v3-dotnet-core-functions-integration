//
// Azure Media Services REST API v3 Functions
//
// ConvertMesPresetJson - This function converts AMSv2 based MES custom preset JSON data to a preset parameters for new Transform (StandardEncoderPreset).
//
/*
```c#
Input:
    {
        "mesPresetJson": "{ \"Version\": 1.0, \"Codecs\": [ { \"KeyFrameInterval\": \"00:00:02\", \"H264Layers\": [ { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 6000, \"MaxBitrate\": 6000, \"BufferWindow\": \"00:00:05\", \"Width\": 1920, \"Height\": 1080, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 4700, \"MaxBitrate\": 4700, \"BufferWindow\": \"00:00:05\", \"Width\": 1920, \"Height\": 1080, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 3400, \"MaxBitrate\": 3400, \"BufferWindow\": \"00:00:05\", \"Width\": 1280, \"Height\": 720, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 2250, \"MaxBitrate\": 2250, \"BufferWindow\": \"00:00:05\", \"Width\": 960, \"Height\": 540, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 1500, \"MaxBitrate\": 1500, \"BufferWindow\": \"00:00:05\", \"Width\": 960, \"Height\": 540, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 1000, \"MaxBitrate\": 1000, \"BufferWindow\": \"00:00:05\", \"Width\": 640, \"Height\": 360, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 650, \"MaxBitrate\": 650, \"BufferWindow\": \"00:00:05\", \"Width\": 640, \"Height\": 360, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" }, { \"Profile\": \"Auto\", \"Level\": \"auto\", \"Bitrate\": 400, \"MaxBitrate\": 400, \"BufferWindow\": \"00:00:05\", \"Width\": 320, \"Height\": 180, \"BFrames\": 3, \"ReferenceFrames\": 3, \"AdaptiveBFrame\": true, \"Type\": \"H264Layer\", \"FrameRate\": \"0/1\" } ], \"Type\": \"H264Video\" }, { \"Profile\": \"AACLC\", \"Channels\": 6, \"SamplingRate\": 48000, \"Bitrate\": 384, \"Type\": \"AACAudio\" }, { \"Start\": \"00:00:00\", \"Step\": \"10%\", \"Range\": \"00:00:10\", \"Type\": \"PngImage\", \"PngLayers\": [ { \"Type\": \"PngLayer\", \"Width\": 640, \"Height\": 360 } ] } ], \"Outputs\": [ { \"FileName\": \"{Basename}_{Width}x{Height}_{VideoBitrate}.mp4\", \"Format\": { \"Type\": \"MP4Format\" } }, { \"FileName\": \"{Basename}_{Index}{Extension}\", \"Format\": { \"Type\": \"PngFormat\" } } ]}"
    }
Output:
    {
        "preset": {
            "@odata.type": "#Microsoft.Media.StandardEncoderPreset",
            "filters": null,
            "codecs": [
                {
                    "@odata.type": "#Microsoft.Media.H264Video",
                    "sceneChangeDetection": false,
                    "complexity": "Balanced",
                    "layers": [
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 6000000,
                            "maxBitrate": 6000000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "1920",
                            "height": "1080",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 4700000,
                            "maxBitrate": 4700000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "1920",
                            "height": "1080",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 3400000,
                            "maxBitrate": 3400000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "1280",
                            "height": "720",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 2250000,
                            "maxBitrate": 2250000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "960",
                            "height": "540",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 1500000,
                            "maxBitrate": 1500000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "960",
                            "height": "540",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 1000000,
                            "maxBitrate": 1000000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "640",
                            "height": "360",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 650000,
                            "maxBitrate": 650000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "640",
                            "height": "360",
                            "label": null
                        },
                        {
                            "profile": "Auto",
                            "level": "auto",
                            "bufferWindow": "PT5S",
                            "referenceFrames": 3,
                            "entropyMode": "Cabac",
                            "bitrate": 400000,
                            "maxBitrate": 400000,
                            "bFrames": 3,
                            "frameRate": "0/1",
                            "slices": 0,
                            "adaptiveBFrame": true,
                            "width": "320",
                            "height": "180",
                            "label": null
                        }
                    ],
                    "keyFrameInterval": "PT2S",
                    "stretchMode": "AutoSize",
                    "label": null
                },
                {
                    "@odata.type": "#Microsoft.Media.AacAudio",
                    "profile": "AacLc",
                    "channels": 6,
                    "samplingRate": 48000,
                    "bitrate": 384000,
                    "label": null
                },
                {
                    "@odata.type": "#Microsoft.Media.PngImage",
                    "layers": [
                        {
                            "width": "640",
                            "height": "360",
                            "label": null
                        }
                    ],
                    "start": "00:00:00",
                    "step": "10%",
                    "range": "00:00:10",
                    "keyFrameInterval": null,
                    "stretchMode": "AutoSize",
                    "label": null
                }
            ],
            "formats": [
                {
                    "@odata.type": "#Microsoft.Media.Mp4Format",
                    "outputFiles": [],
                    "filenamePattern": "{Basename}_{Width}x{Height}_{VideoBitrate}.mp4"
                },
                {
                    "@odata.type": "#Microsoft.Media.PngFormat",
                    "filenamePattern": "{Basename}_{Index}{Extension}"
                }
            ]
        }
    }

```
*/
//
//

using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;
using advanced_vod_functions_v3.SharedLibs.amsv2;


namespace advanced_vod_functions_v3.Functions
{
    public static class ConvertMesPresetJson
    {
        [FunctionName("ConvertMesPresetJson")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - ConvertMesPresetJson was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.mesPresetJson == null)
                return new BadRequestObjectResult("Please pass mesPresetJson in the input object");
            string mesPresetJson = data.mesPresetJson;

            //MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string presetString = null;
            JToken jt = null;

            try
            {
                //IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);
                JsonConverter[] v2JsonConverters = { new MESPresentSchemaJsonConverter() };
                MESPresetSchema v2preset = JsonConvert.DeserializeObject<MESPresetSchema>(mesPresetJson, v2JsonConverters);
                StandardEncoderPreset v3preset = MediaServicesHelperMES.convertMESPreset(v2preset);
                JsonConverter[] v3JsonConverters = { new MediaServicesHelperJsonWriter(), new MediaServicesHelperTimeSpanJsonConverter() };
                presetString = JsonConvert.SerializeObject(v3preset, v3JsonConverters);
                jt = JToken.Parse(presetString);

            }
            catch (ApiErrorException e)
            {
                log.LogError($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.LogError($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                preset = jt
            });
        }
    }
}
