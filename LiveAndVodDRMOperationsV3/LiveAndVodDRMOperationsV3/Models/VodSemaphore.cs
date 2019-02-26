using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveDRMOperationsV3.Models
{
    // Base model for other models.
    // It implements a partitionKey property always set to the same value - used for Cosmos

    public partial class VodSemaphore
    {
        [JsonProperty("encodedAsset")]
        public bool EncodedAsset { get; set; }

        [JsonProperty("transformName", NullValueHandling = NullValueHandling.Ignore)]
        public string TransformName { get; set; }

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public string StartTime { get; set; }

        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
        public string EndTime { get; set; }

        [JsonProperty("urn", NullValueHandling = NullValueHandling.Ignore)]
        public string Urn { get; set; }

        [JsonProperty("drmContentId", NullValueHandling = NullValueHandling.Ignore)]
        public string DrmContentId { get; set; }

        [DefaultValue(false)]
        [JsonProperty("clearStream", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ClearStream { get; set; }

        [JsonProperty("files")]
        public File[] Files { get; set; }
    }

    public partial class File
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [DefaultValue(false)]
        [JsonProperty("containsVideo", NullValueHandling = NullValueHandling.Ignore)]
        public bool ContainsVideo { get; set; }

        [DefaultValue(false)]
        [JsonProperty("containsAudio", NullValueHandling = NullValueHandling.Ignore)]
        public bool ContainsAudio { get; set; }

        [JsonProperty("videoCodec", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoCodec { get; set; }

        [JsonProperty("audioCodec", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioCodec { get; set; }

        [JsonProperty("mediaContainer", NullValueHandling = NullValueHandling.Ignore)]
        public string MediaContainer { get; set; }

        [JsonProperty("videoQuality", NullValueHandling = NullValueHandling.Ignore)]
        public string VideoQuality { get; set; }

        [JsonProperty("audioLanguage", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioLanguage { get; set; }

        [JsonProperty("audioTitle ", NullValueHandling = NullValueHandling.Ignore)]
        public string AudioTitle { get; set; }
       
        [JsonProperty("containsText", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ContainsText { get; set; }

        [JsonProperty("language", NullValueHandling = NullValueHandling.Ignore)]
        public string Language { get; set; }

        [JsonProperty("textLanguage", NullValueHandling = NullValueHandling.Ignore)]
        public string TextLanguage { get; set; }

        [JsonProperty("textTitle", NullValueHandling = NullValueHandling.Ignore)]
        public string TextTitle { get; set; }

        [DefaultValue(false)]
        [JsonProperty("copyToSubAsset")]
        public bool CopyToSubAsset { get; set; }
    }

    public partial class VodSemaphore
    {
        public static VodSemaphore FromJson(string json) => JsonConvert.DeserializeObject<VodSemaphore>(json, LiveDRMOperationsV3.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this VodSemaphore self) => JsonConvert.SerializeObject(self, LiveDRMOperationsV3.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}


/* SAMPLE JSON

{
  "encodedAsset": true, // true : no additional encoded is needed on Azure but a manifest must be generated, false: encoded is needed
  "transformName": "myTransform", // ask to process the asset with a AMS v3 Transform. For encoding or other task like speech analysis
  "startTime": "date-time", // ISO 8601 start date and time of the availability of the URL, the stream cannot be played before; "now" if property not present. Parsing : https://docs.microsoft.com/en-us/dotnet/api/system.datetime?view=netframework-4.7.2#parsing-03 
  "endTime": "date-time", // ISO 8601 end date and time of the availability of the URL, the stream is no longer available after this time; "31/12/9999 23:59.59" if property not present
  "urn": "asset URN", // URN of Asset; Filename if property not present
  "drmContentId": "vodMajor1", // Content ID used to register the key in the DRM and to generate the License URL; variable in logi app is used if value not present
  "clearStream": true, // true : stream is not DRM protected, false : stream is DRM protected; false if property not present
  "files": [
    {
      "fileName": "video-400.mp4",
      "containsVideo": true, // false if property not present
      "containsAudio": true, // false if property not present
      "videoCodec":"H264", // ignored if not present
      "audioCodec":"AAC", // ignored if not present
      "mediaContainer": "MP4", // ignored if not present
      "videoQuality": "SD" // Quality = SD for resolutions with less than 720 lines; Quality = HD for 720 and 1080 lines; Quality = UHD for more than 1080 lines; SD if property not present
    },
    {
      "fileName": "video-700.mp4",
      "containsVideo": true,
      "containsAudio": true,
      "audioLanguage": "deu", // based on ISO 639-2 language codes; if not preset and audio track is present, then audioLanguage = "und" (undefined)
      "audioTitle ": "German audio" // ignored if not present
    },
    {
      "fileName": "video-1200.mp4",
      "containsVideo": true,
      "containsAudio": true,
      "audioLanguage": "eng"
    },
    {
      "fileName": "subtitle-de.vtt",
      "containsText": true, // false if property not present
      "textLanguage": "deu",
      "copyToSubAsset" : true  // it means we want this file to be copied in another asset which will be published with clear policy (default is false)
    },
    {
      "fileName": "subtitle-en.vtt",
      "containsText": true,
      "textLanguage": "eng",
      "textTitle": "English subtitles", // ignored if not present
      "copyToSubAsset" : true  // it means we want this file to be copied in another asset which will be published with clear policy (default is false)
    },
    {
      "fileName": "audio-en.mp4",
      "containsAudio": true,
      "audioLanguage ": "eng"
    }
  ]
}


    https://app.quicktype.io/#l=cs&r=json2csharp

*/
