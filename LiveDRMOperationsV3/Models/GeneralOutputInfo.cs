using System.Collections.Generic;
using System.Reflection;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;

namespace LiveDrmOperationsV3.Models
{
    
    public class Drm
    {
        [JsonProperty("type")] public string Type { get; set; }

        [JsonProperty("licenseUrl")] public string LicenseUrl { get; set; }

        [JsonProperty("certificateUrl", NullValueHandling = NullValueHandling.Ignore)] public string CertificateUrl { get; set; }

        [JsonProperty("protocols")] public List<string> Protocols { get; set; }
    }

    public class UrlEntry
    {
        [JsonProperty("protocol")] public string Protocol { get; set; }

        [JsonProperty("url")] public string Url { get; set; }
    }

    public class StreamingLocatorEntry
    {
        [JsonProperty("streamingLocatorName")] public string StreamingLocatorName { get; set; }

        [JsonProperty("streamingPolicyName")] public string StreamingPolicyName { get; set; }

        [JsonProperty("cencKeyId")] public string CencKeyId { get; set; }

        [JsonProperty("cbcsKeyId")] public string CbcsKeyId { get; set; }

        [JsonProperty("drm")] public List<Drm> Drm { get; set; }

        [JsonProperty("urls")] public List<UrlEntry> Urls { get; set; }
    }

    public class LiveOutputEntry
    {
        [JsonProperty("liveOutputName")] public string LiveOutputName { get; set; }

        [JsonProperty("archiveWindowLength")] public int ArchiveWindowLength { get; set; }

        [JsonProperty("assetName")] public string AssetName { get; set; }

        [JsonProperty("assetStorageAccountName")] public string AssetStorageAccountName { get; set; }

        [JsonProperty("resourceState")] public LiveOutputResourceState? ResourceState { get; set; }

        [JsonProperty("streamingLocators")] public List<StreamingLocatorEntry> StreamingLocators { get; set; }
    }

    public class LiveEventEntry
    {
        [JsonProperty("liveEventName")] public string LiveEventName { get; set; }

        [JsonProperty("resourceState")] public string ResourceState { get; set; }

        [JsonProperty("vanityUrl")] public bool? VanityUrl { get; set; }

        [JsonProperty("amsAccountName")] public string AMSAccountName { get; set; }

        [JsonProperty("region")] public string Region { get; set; }

        [JsonProperty("resourceGroup")] public string ResourceGroup { get; set; }

        [JsonProperty(PropertyName = "lowLatency")] public bool? LowLatency { get; set; }

        [JsonProperty(PropertyName = "id")] public string Id => AMSAccountName + ":" + LiveEventName;


        [JsonProperty("input")] public List<UrlEntry> Input { get; set; }

        [JsonProperty("inputACL")] public List<string> InputACL { get; set; }

        [JsonProperty("preview")] public List<UrlEntry> Preview { get; set; }

        [JsonProperty("previewACL")] public List<string> PreviewACL { get; set; }

        [JsonProperty("liveOutputs")] public List<LiveOutputEntry> LiveOutputs { get; set; }
    }

    public class GeneralOutputInfo
    {
        [JsonProperty("success")] public bool Success { get; set; }

        [JsonProperty("operationsVersion")]
        public string OperationsCodeVersion =>
            AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        [JsonProperty("liveEvents")] public List<LiveEventEntry> LiveEvents { get; set; }
    }
}