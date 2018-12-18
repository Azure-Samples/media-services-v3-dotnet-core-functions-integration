//
// Azure Media Services REST API v3 Functions
//
// CreateTransform - This function creates a new transform.
//
/*
```c#
Input:
    {
        // [Required] The name of the transform.
        "transformName": "TestTransform",

        // [Required] The mode for creating the transform.
        // Allowed values: "simple" or "advanced".
        // Default value: "simple".
        "mode": "simple",

        // The description of the transform.
        "description": "Transform for testing",

        //
        // [mode = simple]
        //
        // [Required] Preset that describes the operations
        // that will be used to modify, transcode, or extract insights
        // from the source file to generate the transform output.
        // Allowed values:
        //  H264SingleBitrateSD, H264SingleBitrate720p, H264SingleBitrate1080p,
        //  AdaptiveStreaming, AACGoodQualityAudio,
        //  H264MultipleBitrate1080p, H264MultipleBitrate720p, H264MultipleBitrateSD,
        //  AudioAnalyzer, VideoAnalyzer,
        //  CustomPreset.
        "preset": "AdaptiveStreaming",

        //
        // [mode = simple]
        //
        // The JSON representing a custom preset.
        // See https://docs.microsoft.com/rest/api/media/transforms/createorupdate#standardencoderpreset
        // for further details on the settings to use to build a custom preset.
        "customPresetJson": { ... },

        //
        // [mode = simple]
        //
        // A Transform can define more than one output.
        // This property defines what the service should do when one output fails -
        // either continue to produce other outputs, or, stop the other outputs.
        // The overall Job state will not reflect failures of outputs that are specified with 'ContinueJob'.
        // The default is 'StopProcessingJob'.
        // Allowed values: ContinueJob, StopProcessingJob.
        "onError": "StopProcessingJob",

        //
        // [mode = simple]
        //
        // Sets the relative priority of the transform outputs within a transform.
        // This sets the priority that the service uses for processing TransformOutputs.
        // The default priority is Normal.
        // Allowed values: High, Low, Normal.
        "relativePriority": "Normal",

        //
        // [mode = simple & preset = AudioAnalyzer | VideoAnalyzer]
        //
        // The language for the audio payload in the input using the BCP-47 format of "language tag-region" (e.g: en-US).
        // If not specified, automatic language detection would be employed.
        // This feature currently supports English, Chinese, French, German,
        // Italian, Japanese, Spanish, Russian, and Portuguese.
        // The automatic detection works best with audio recordings with clearly discernable speech.
        // If automatic detection fails to find the language, transcription would fallback to English.
        // Allowed values: en-US, en-GB, es-ES, es-MX, fr-FR, it-IT, ja-JP, pt-BR, zh-CN, de-DE, ar-EG, ru-RU, hi-IN.
        "audioLanguage": "en-US",

        //
        // [mode = simple & preset = VideoAnalyzer]
        //
        // The type of insights to be extracted.
        // If not set then the type will be selected based on the content type.
        // If the content is audio only then only audio insights will be extracted
        // and if it is video only video insights will be extracted.
        // Allowed values: AllInsights, AudioInsightsOnly, VideoInsightsOnly.
        "insightsToExtract": "AllInsights",

        //
        // [mode = advanced]
        //
        // [Required] The array of custom presets for the transform.
        "transformOutputs": [
            {
                "onError": "StopProcessingJob",
                "relativePriority": "Normal",
                "preset": {
                    "@odata.type": "#Microsoft.Media.BuiltInStandardEncoderPreset",
                    "presetName": "AdaptiveStreaming"
                }
            },
            {
                "onError": "StopProcessingJob",
                "relativePriority": "Normal",
                "preset": {
                    "@odata.type": "#Microsoft.Media.VideoAnalyzerPreset",
                    "audioLanguage": "en-US",
                    "audioInsightsOnly": false
                }
            }
        ]
    }
Output:
    {
        // The name of the created TransformName
        "transformName": "TestTransform",

        // The resource identifier of the created Transform
        "transformId": "/subscriptions/694d5930-8ee4-4e50-917b-9dcfeceb6179/resourceGroups/AMSdemo/providers/Microsoft.Media/mediaservices/amsdemojapaneast/transforms/TestTransform"
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class CreateTransform
    {
        [FunctionName("CreateTransform")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - CreateTransform was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.transformName == null)
                return new BadRequestObjectResult("Please pass transformName in the input object");
            string transformName = data.transformName;
            if (data.mode == null)
                return new BadRequestObjectResult("Please pass mode in the input object");
            string description = null;
            if (data.description != null)
                description = data.description;

            string mode = data.mode;
            if (mode != "simple" && mode != "advanced")
                return new BadRequestObjectResult("Please pass valid mode in the input object");
            //
            // Simple Mode
            //
            if (mode == "simple" && data.preset == null)
                return new BadRequestObjectResult("Please pass preset in the input object");
            string presetName = data.preset;
            if (presetName == "CustomPreset" && data.customPresetJson == null)
                return new BadRequestObjectResult("Please pass customPresetJson in the input object");
            //
            // Advanced Mode
            //
            if (mode == "advanced" && data.transformOutputs == null)
                return new BadRequestObjectResult("Please pass transformOutputs in the input object");

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string transformId = null;

            JsonConverter[] jsonReaders = {
                new MediaServicesHelperJsonReader(),
                new MediaServicesHelperTimeSpanJsonConverter()
            };

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                // Does a Transform already exist with the desired name?
                // Assume that an existing Transform with the desired name
                // also uses the same recipe or Preset for processing content.
                Transform transform = client.Transforms.Get(amsconfig.ResourceGroup, amsconfig.AccountName, transformName);
                if (transform == null)
                {
                    TransformOutput[] outputs = null;
                    List<TransformOutput> transformOutputs = new List<TransformOutput>();
                    if (mode == "simple")
                    {
                        Preset preset = null;
                        string audioLanguage = null;
                        if (data.audioLanguage != null)
                            audioLanguage = data.audioLanguage;
                        if (presetName == "VideoAnalyzer")
                        {
                            bool insightEnabled = false;
                            if (data.insightsToExtract != null && InsightTypeList.ContainsKey(data.insightsToExtract))
                                insightEnabled = true;
                            preset = new VideoAnalyzerPreset(audioLanguage, insightEnabled ? InsightTypeList[data.insightsToExtract] : null);
                        }
                        else if (presetName == "AudioAnalyzer")
                        {
                            preset = new AudioAnalyzerPreset(audioLanguage);
                        }
                        else if (presetName == "CustomPreset")
                        {
                            preset = JsonConvert.DeserializeObject<StandardEncoderPreset>(data.customPresetJson.ToString(), jsonReaders);
                        }
                        else {
                            if (!EncoderNamedPresetList.ContainsKey(presetName))
                                return new BadRequestObjectResult("Preset not found");
                            preset = new BuiltInStandardEncoderPreset(EncoderNamedPresetList[presetName]);
                        }
                        OnErrorType onError = OnErrorType.StopProcessingJob;
                        if (data.onError != null && OnErrorTypeList.ContainsKey(data.onError))
                            onError = OnErrorTypeList[data.onError];
                        Priority relativePriority = Priority.Normal;
                        if (data.relativePriority != null && PriorityList.ContainsKey(data.relativePriority))
                            relativePriority = PriorityList[data.relativePriority];
                        transformOutputs.Add(new TransformOutput(preset, onError, relativePriority));
                        outputs = transformOutputs.ToArray();
                    }
                    else if (mode == "advanced")
                    {
                        List<TransformOutput> transformOutputList = JsonConvert.DeserializeObject<List<TransformOutput>>(data.transformOutputs.ToString(), jsonReaders);
                        outputs = transformOutputList.ToArray();
                    }
                    else
                    {
                        return new BadRequestObjectResult("Invalid mode found");
                    }

                    // Create Transform
                    transform = client.Transforms.CreateOrUpdate(amsconfig.ResourceGroup, amsconfig.AccountName, transformName, outputs);
                }
                transformId = transform.Id;
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
                transformName = transformName,
                transformId = transformId
            });
        }

        private static Dictionary<string, OnErrorType> OnErrorTypeList = new Dictionary<string, OnErrorType>()
        {
            { "ContinueJob", OnErrorType.ContinueJob },
            { "StopProcessingJob", OnErrorType.StopProcessingJob }
        };
        private static Dictionary<string, Priority> PriorityList = new Dictionary<string, Priority>()
        {
            { "High", Priority.High },
            { "Low", Priority.Low },
            { "Normal", Priority.Normal }
        };
        private static Dictionary<string, InsightsType> InsightTypeList = new Dictionary<string, InsightsType>()
        {
            { "AllInsights", InsightsType.AllInsights },
            { "AudioInsightsOnly", InsightsType.AudioInsightsOnly },
            { "VideoInsightsOnly", InsightsType.VideoInsightsOnly }
        };
        private static Dictionary<string, EncoderNamedPreset> EncoderNamedPresetList = new Dictionary<string, EncoderNamedPreset>()
        {
            { "AACGoodQualityAudio", EncoderNamedPreset.AACGoodQualityAudio },
            { "AdaptiveStreaming", EncoderNamedPreset.AdaptiveStreaming },
            { "H264MultipleBitrate1080p", EncoderNamedPreset.H264MultipleBitrate1080p },
            { "H264MultipleBitrate720p", EncoderNamedPreset.H264MultipleBitrate720p },
            { "H264MultipleBitrateSD", EncoderNamedPreset.H264MultipleBitrateSD },
            { "H264SingleBitrate1080p", EncoderNamedPreset.H264SingleBitrate1080p },
            { "H264SingleBitrate720p", EncoderNamedPreset.H264SingleBitrate720p },
            { "H264SingleBitrateSD", EncoderNamedPreset.H264SingleBitrateSD }
        };
    }
}
