using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using LiveDrmOperationsV3.Helpers;
using LiveDRMOperationsV3.Models;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveDrmOperationsV3.Models
{

    public partial class DrmList
    {

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("licenseUrl")]
        public string LicenseUrl { get; set; }

        [JsonProperty("certificateUrl")]
        public string CertificateUrl { get; set; }
    }

    public partial class Subtitles
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("textLanguage")]
        public string TextLanguage { get; set; }

        [JsonProperty("textTitle")]
        public string TextTitle { get; set; }
    }

    public partial class ResourceList
    {

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("drmList")]
        public IList<DrmList> DrmList { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("videoCodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("audioCodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("mediaContainer")]
        public string MediaContainer { get; set; }

        [JsonProperty("live")]
        public bool Live { get; set; }

        [JsonProperty("subTitles")]
        public IList<Subtitles> SubTitles { get; set; }
    }

    public partial class VodResource : BaseModel
    {

        [JsonProperty("urn")]
        public string Urn { get; set; }

        [JsonProperty("resourceList")]
        public IList<ResourceList> ResourceList { get; set; }

        [JsonIgnore] public AssetEntry MainAsset { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id => ((new Uri(ResourceList[0]?.Url)).Host + "-" + (new Uri(ResourceList[0]?.Url)).Segments[1].Replace("/", string.Empty)).ToLower();

        [JsonProperty(PropertyName = "partitionKey", NullValueHandling = NullValueHandling.Ignore)]
        public override string PartitionKey => (MainAsset.Semaphore.DrmContentId ?? DefaultPartitionValue).ToLower();

        public new static string DefaultPartitionValue = "vod";
    }

}