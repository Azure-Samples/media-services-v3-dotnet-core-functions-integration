using Newtonsoft.Json;

namespace LiveDRMOperationsV3.Models
{
    // Base model for other models.
    // It implements a partitionKey property always set to the same value - used for Cosmos
    public class BaseModel
    {
        public static string DefaultPartitionValue = "live";

        [JsonProperty(PropertyName = "partitionKey", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string PartitionKey => DefaultPartitionValue;
    }
}