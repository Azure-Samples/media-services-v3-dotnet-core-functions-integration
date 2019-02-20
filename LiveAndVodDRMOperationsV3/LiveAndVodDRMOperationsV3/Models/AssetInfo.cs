using System.Collections.Generic;
using System.Reflection;
using LiveDrmOperationsV3.Helpers;
using LiveDRMOperationsV3.Models;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;

namespace LiveDrmOperationsV3.Models
{

    public class AssetEntry : BaseModel
    {
        public static readonly string DateFormat = "yyyyMMddTHH:mm:ssZ";

        [JsonProperty("assetName")] public string AssetName { get; set; }

        [JsonProperty("assetStorageAccountName")] public string AssetStorageAccountName { get; set; }

        [JsonProperty("streamingLocators")] public List<StreamingLocatorEntry> StreamingLocators { get; set; }

        [JsonProperty("amsAccountName")] public string AMSAccountName { get; set; }

        [JsonProperty("region")] public string Region { get; set; }

        [JsonProperty("resourceGroup")] public string ResourceGroup { get; set; }

        [JsonProperty("urn")] public string Urn { get; set; }

        [JsonProperty("createdTime")] public string CreatedTime { get; set; }

        [JsonProperty("semaphore")] public object Semaphore { get; set; }

        [JsonIgnore] public string ContentId { get; set; }

        [JsonIgnore] public string RegionCode { get; set; }

        [JsonProperty(PropertyName = "id")] public string Id => (AMSAccountName + ":" + AssetName).ToLower();

        [JsonProperty(PropertyName = "partitionKey")] public override string PartitionKey => ContentId ?? "UND" + RegionCode;
    }


    public class AssetInfo
    {
        [JsonProperty("success")] public bool Success { get; set; }

        [JsonProperty("operationsVersion")]
        public string OperationsCodeVersion =>
            AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();

        [JsonProperty("assets")] public List<AssetEntry> Assets { get; set; }
    }
}