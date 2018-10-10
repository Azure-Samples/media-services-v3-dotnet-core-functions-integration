using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace LiveDrmOperationsV3.Models
{
    public class Input
    {
        [JsonProperty("Protocol")]
        public string Protocol { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }
    }

    public class Preview
    {
        [JsonProperty("Protocol")]
        public string Protocol { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }
    }

    public class Drm
    {
        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("LicenseUrl")]
        public string LicenseUrl { get; set; }
    }

    public class UrlEntry
    {
        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Protocol")]
        public string Protocol { get; set; }
    }

    public class StreamingLocatorEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("StreamingPolicyName")]
        public string StreamingPolicyName { get; set; }

        [JsonProperty("CencKeyId")]
        public string CencKeyId { get; set; }

        [JsonProperty("CbcsKeyId")]
        public string CbcsKeyId { get; set; }

        [JsonProperty("Drm")]
        public List<Drm> Drm { get; set; }

        [JsonProperty("Urls")]
        public List<UrlEntry> Urls { get; set; }
    }

    public class LiveOutputEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ArchiveWindowLength")]
        public int ArchiveWindowLength { get; set; }

        [JsonProperty("AssetName")]
        public string AssetName { get; set; }

        [JsonProperty("AssetStorageAccountName")]
        public string AssetStorageAccountName { get; set; }

        [JsonProperty("ResourceState")]
        public LiveOutputResourceState? ResourceState { get; set; }

        [JsonProperty("StreamingLocators")]
        public List<StreamingLocatorEntry> StreamingLocators { get; set; }
    }

    public class LiveEventEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ResourceState")]
        public string ResourceState { get; set; }

        [JsonProperty("VanityUrl")]
        public bool? VanityUrl { get; set; }

        [JsonProperty("Input")]
        public List<Input> Input { get; set; }

        [JsonProperty("InputACL")]
        public List<string> InputACL { get; set; }

        [JsonProperty("Preview")]
        public List<Preview> Preview { get; set; }

        [JsonProperty("PreviewACL")]
        public List<string> PreviewACL { get; set; }

        [JsonProperty("LiveOutputs")]
        public List<LiveOutputEntry> LiveOutputs { get; set; }

        [JsonProperty("AMSAccountName")]
        public string AMSAccountName { get; set; }

        [JsonProperty("Region")]
        public string Region { get; set; }

        [JsonProperty("ResourceGroup")]
        public string ResourceGroup { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return AMSAccountName + ":" + Name;
            }
        }
    }

    public class GeneralOutputInfo
    {
        [JsonProperty("Success")]
        public bool Success { get; set; }

        [JsonProperty("OperationsVersion")]
        public string OperationsCodeVersion
        {
            get
            {
                return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();
            }
        }

        [JsonProperty("LiveEvents")]
        public List<LiveEventEntry> LiveEvents { get; set; }

    }
}
