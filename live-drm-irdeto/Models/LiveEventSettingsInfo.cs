using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LiveDrmOperationsV3.Models
{
    public class Dvr
    {
        [JsonProperty("enableByDefault")]
        public string enableByDefault { get; set; }

        [JsonProperty("allowed")]
        public string allowed { get; set; }
    }
    public class ResourceListOrder
    {
        [JsonProperty("dvr")]
        public Dvr dvr { get; set; }

        [JsonProperty("encryptionSorted")]
        public string encryptionSorted { get; set; }

        [JsonProperty("defaultAzureRegion")]
        public string defaultAzureRegion { get; set; }
    }

    public class PlayerJSONData
    {
        [JsonProperty("quality")]
        public string quality { get; set; }

        [JsonProperty("encoding")]
        public string encoding { get; set; }

        [JsonProperty("presentation")]
        public string presentation { get; set; }

        [JsonProperty("live")]
        public bool live { get; set; }

        [JsonProperty("mediaContainer")]
        public string mediaContainer { get; set; }

        [JsonProperty("audioCodec")]
        public string audioCodec { get; set; }

        [JsonProperty("videoCodec")]
        public string videoCodec { get; set; }

        [JsonProperty("resourceListOrder")]
        public ResourceListOrder resourceListOrder { get; set; }
    }

    public class LiveEventSettingsInfo
    {
        public LiveEventSettingsInfo()
        {
            archiveWindowLength = 10;
            vanityUrl = false;
            inputProtocol = LiveEventInputProtocol.FragmentedMP4;
            autoStart = true;
        }

        [JsonProperty("liveEventName")]
        public string liveEventName { get; set; }

        [JsonProperty("urn")]
        public string urn { get; set; }

        [JsonProperty("vendor")]
        public string vendor { get; set; }

        [JsonProperty("akamaiHostname")]
        public string akamaiHostname { get; set; }

        [JsonProperty("baseStorageName")]
        public string baseStorageName { get; set; }

        [JsonIgnore]
        public string StorageName { get; set; }

        [JsonProperty("archiveWindowLength")]
        public int archiveWindowLength { get; set; }

        [JsonProperty("vanityUrl")]
        public bool vanityUrl { get; set; }

        [JsonIgnore]
        public LiveEventInputProtocol inputProtocol { get; set; }

        [JsonIgnore]
        public bool autoStart { get; set; }

        [JsonProperty("liveEventInputACL")]
        public IList<string> liveEventInputACL { get; set; }

        [JsonProperty("liveEventPreviewACL")]
        public IList<string> liveEventPreviewACL { get; set; }

        [JsonProperty("playerJSONData")]
        public PlayerJSONData playerJSONData { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return liveEventName;
            }
        }
    }
}
