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

    public partial class AssetEntry : BaseModel
    {
        public static readonly string DateFormat = "yyyyMMddTHH:mm:ssZ";

        public new static string DefaultPartitionValue = "vod";

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("assetStorageAccountName", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetStorageAccountName { get; set; }

        [JsonProperty("streamingLocators", NullValueHandling = NullValueHandling.Ignore)]
        public List<StreamingLocatorEntry> StreamingLocators { get; set; }

        [JsonProperty("amsAccountName", NullValueHandling = NullValueHandling.Ignore)]
        public string AMSAccountName { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        [JsonProperty("resourceGroup", NullValueHandling = NullValueHandling.Ignore)]
        public string ResourceGroup { get; set; }

        [JsonProperty("urn", NullValueHandling = NullValueHandling.Ignore)]
        public string Urn { get; set; }

        [JsonProperty("createdTime", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedTime { get; set; }

        [JsonProperty("semaphore", NullValueHandling = NullValueHandling.Ignore)]
        public VodSemaphore Semaphore { get; set; }

        [JsonIgnore] public string ContentId { get; set; }

        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id => (AMSAccountName + ":" + AssetName).ToLower();

        [JsonProperty(PropertyName = "partitionKey", NullValueHandling = NullValueHandling.Ignore)]
        public override string PartitionKey => (ContentId ?? "UND").ToLower();
    }


    public partial class AssetEntry
    {
        public static AssetEntry FromJson(string json) => JsonConvert.DeserializeObject<AssetEntry>(json, Converter.Settings);
    }


    public class VodAssetInfo
    {
        [JsonProperty("success")] public bool Success { get; set; }

        [JsonProperty("operationsVersion")]
        public string OperationsCodeVersion =>
            AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        [JsonProperty("assets")] public List<AssetEntry> Assets { get; set; }
    }

    public class VodAssetInfoSimple
    {
        [JsonProperty("success")] public bool Success { get; set; }

        [JsonProperty("operationsVersion")]
        public string OperationsCodeVersion =>
            AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        [JsonProperty("createdLocatorName")] public string CreatedLocatorName { get; set; }

        [JsonProperty("createdLocatorPath")] public string CreatedLocatorPath { get; set; }

        [JsonProperty("asset")] public AssetEntry Asset { get; set; }
    }

}