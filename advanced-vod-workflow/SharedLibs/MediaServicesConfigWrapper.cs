//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;

namespace advanced_vod_functions_v3.SharedLibs
{
    public class MediaServicesConfigWrapper
    {
        public MediaServiceClientCredentials mediaServiceClientCredentials = new MediaServiceClientCredentials();

        public string SubscriptionId
        {
            get { return Environment.GetEnvironmentVariable("SubscriptionId"); }
        }

        public string ResourceGroup
        {
            get { return Environment.GetEnvironmentVariable("ResourceGroup"); }
        }

        public string AccountName
        {
            get { return Environment.GetEnvironmentVariable("AccountName"); }
        }

        public string Region
        {
            get { return Environment.GetEnvironmentVariable("Region"); }
        }
    }
}
