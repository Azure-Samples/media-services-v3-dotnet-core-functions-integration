//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;

namespace advanced_vod_functions_v3.SharedLibs
{
    public class MediaServiceClientCredentials
    {
        public Uri AadEndpoint
        {
            get { return new Uri(Environment.GetEnvironmentVariable("AadEndpoint")); }
        }

        public string AadTenantId
        {
            get { return Environment.GetEnvironmentVariable("AadTenantId"); }
        }

        public string AadClientId
        {
            get { return Environment.GetEnvironmentVariable("AadClientId"); }
        }

        public string AadClientSecret
        {
            get { return Environment.GetEnvironmentVariable("AadClientSecret"); }
        }

        public Uri ArmEndpoint
        {
            get { return new Uri(Environment.GetEnvironmentVariable("ArmEndpoint")); }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(Environment.GetEnvironmentVariable("ArmAadAudience")); }
        }
    }
}
