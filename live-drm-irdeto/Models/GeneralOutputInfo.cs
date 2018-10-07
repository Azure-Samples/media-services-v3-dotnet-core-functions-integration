using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiveDRMIrdeto.Models
{
    public class Input
    {
        public string Protocol { get; set; }
        public string Url { get; set; }
    }

    public class Preview
    {
        public string Protocol { get; set; }
        public string Url { get; set; }
    }

    public class Drm
    {
        public string Type { get; set; }
        public string LicenseUrl { get; set; }
    }

    public class UrlEntry
    {
        public string Url { get; set; }
        public string Protocol { get; set; }
    }

    public class LiveOutputEntry
    {
        public string Name { get; set; }
        public int ArchiveWindowLength { get; set; }
        public string AssetName { get; set; }
        public string AssetStorageAccountName { get; set; }
        public LiveOutputResourceState? ResourceState { get; set; }
        public string StreamingLocatorName { get; set; }
        public string StreamingPolicyName { get; set; }
        public List<Drm> Drm { get; set; }
        public string CencKeyId { get; set; }
        public string CbcsKeyId { get; set; }
        public List<UrlEntry> Urls { get; set; }
    }

    public class LiveEventEntry
    {
        public string Name { get; set; }
        public string ResourceState { get; set; }
        public bool? vanityUrl { get; set; }
        public List<Input> Input { get; set; }
        public List<string> InputACL { get; set; }
        public List<Preview> Preview { get; set; }
        public List<string> PreviewACL { get; set; }
        public List<LiveOutputEntry> LiveOutputs { get; set; }
        public string AMSAccountName { get; set; }
        public string Region { get; set; }
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
        public bool Success { get; set; }
        public List<LiveEventEntry> LiveEvents { get; set; }
    }
}
