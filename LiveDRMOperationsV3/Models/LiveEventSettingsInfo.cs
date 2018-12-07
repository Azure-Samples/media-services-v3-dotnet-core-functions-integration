using System.Collections.Generic;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;

namespace LiveDrmOperationsV3.Models
{
    public class RedirectorStreamingEndpointData
    {
        [JsonProperty("streamingEndpointName")] public string StreamingEndpointName { get; set; }

        [JsonProperty("percentage")] public int Percentage { get; set; }
    }

    public class Dvr
    {
        [JsonProperty("enableByDefault")] public bool EnableByDefault { get; set; }

        [JsonProperty("allowed")] public bool Allowed { get; set; }
    }

    public class ResourceListOrder
    {
        [JsonProperty("dvr")] public Dvr Dvr { get; set; }

        [JsonProperty("encryptionSorted")] public bool EncryptionSorted { get; set; }

        [JsonProperty("defaultAzureRegion")] public string DefaultAzureRegion { get; set; }
    }

    public class PlayerJSONData
    {
        [JsonProperty("quality")] public string Quality { get; set; }

        [JsonProperty("encoding")] public string Encoding { get; set; }

        [JsonProperty("presentation")] public string Presentation { get; set; }

        [JsonProperty("live")] public bool Live { get; set; }

        [JsonProperty("mediaContainer")] public string MediaContainer { get; set; }

        [JsonProperty("audioCodec")] public string AudioCodec { get; set; }

        [JsonProperty("videoCodec")] public string VideoCodec { get; set; }

        [JsonProperty("resourceListOrder")] public ResourceListOrder ResourceListOrder { get; set; }
    }

    public class LiveEventSettingsInfo
    {
        public LiveEventSettingsInfo()
        {
            ArchiveWindowLength = 10;
            VanityUrl = false;
            InputProtocol = LiveEventInputProtocol.FragmentedMP4;
            AutoStart = true;
            LowLatency = false;
        }

        [JsonProperty("liveEventName")] public string LiveEventName { get; set; }

        [JsonProperty("urn")] public string Urn { get; set; }

        [JsonProperty("vendor")] public string Vendor { get; set; }

        [JsonProperty("akamaiHostname")] public string AkamaiHostname { get; set; }

        [JsonProperty("baseStorageName")] public string BaseStorageName { get; set; }

        [JsonIgnore] public string StorageName { get; set; }

        [JsonProperty("archiveWindowLength")] public int ArchiveWindowLength { get; set; }

        [JsonProperty("vanityUrl")] public bool VanityUrl { get; set; }

        [JsonProperty("lowLatency")] public bool LowLatency { get; set; }

        [JsonIgnore] public LiveEventInputProtocol InputProtocol { get; set; }

        [JsonIgnore] public bool AutoStart { get; set; }

        [JsonProperty("liveEventInputACL")] public IList<string> LiveEventInputACL { get; set; }

        [JsonProperty("liveEventPreviewACL")] public IList<string> LiveEventPreviewACL { get; set; }

        [JsonProperty("playerJSONData")] public PlayerJSONData PlayerJSONData { get; set; }

        [JsonProperty("redirectorStreamingEndpointData")] public List<RedirectorStreamingEndpointData> RedirectorStreamingEndpointData { get; set; }
                
        [JsonProperty(PropertyName = "id")] public string Id => LiveEventName;
    }
}