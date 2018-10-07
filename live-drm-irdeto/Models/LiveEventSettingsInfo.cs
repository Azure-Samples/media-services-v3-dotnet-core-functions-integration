using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveDRMIrdeto.Models
{
    public class PlayerJSONData
    {
        public string quality { get; set; }
        public string encoding { get; set; }
        public string presentation { get; set; }
        public bool dvr { get; set; }
        public bool live { get; set; }
        public string mediaContainer { get; set; }
        public string audioCodec { get; set; }
        public string videoCodec { get; set; }
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


        public string liveEventName { get; set; }
        public string uRN { get; set; }
        public string akamaiHostname { get; set; }
        public string baseStorageName { get; set; }
        [JsonIgnore]
        public string StorageName { get; set; }
        public int archiveWindowLength { get; set; }
        public bool vanityUrl { get; set; }
        [JsonIgnore]
        public LiveEventInputProtocol inputProtocol { get; set; }
        [JsonIgnore]
        public bool autoStart { get; set; }
        public List<string> liveEventInputACL { get; set; }
        public List<string> liveEventPreviewACL { get; set; }
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
