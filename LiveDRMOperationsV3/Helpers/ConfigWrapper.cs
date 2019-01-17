using System;
using Microsoft.Extensions.Configuration;

namespace LiveDrmOperationsV3.Helpers
{
    public class ConfigWrapper
    {
        private readonly IConfiguration _config;

        public ConfigWrapper(IConfiguration config, string azureRegion = null)
        {
            _config = config;
            AzureRegionCode = azureRegion ?? _config["AzureRegion"];
        }

        public string SubscriptionId => _config["SubscriptionId"];

        public string ResourceGroupFinalName => _config["ResourceGroupFinalName"];

        public string ResourceGroup => _config["ResourceGroup"] + (string.IsNullOrEmpty(ResourceGroupFinalName) ? AzureRegionCode : "");

        public string AccountName => _config["AccountName"] + AzureRegionCode;

        public string AadTenantId => _config["AadTenantId"];

        public string AadClientId => _config["AadClientId"];

        public string AadSecret => _config["AadSecret"];

        public Uri ArmAadAudience => _config["ArmAadAudience"] != null ? new Uri(_config["ArmAadAudience"]) : null;

        public Uri AadEndpoint => _config["AadEndpoint"] != null ? new Uri(_config["AadEndpoint"]) : null;

        public Uri ArmEndpoint => _config["ArmEndpoint"] != null ? new Uri(_config["ArmEndpoint"]) : null;

        public string Region
        {
            get
            {
                if (AzureRegionCode == null)
                    return _config["Region"];
                switch (AzureRegionCode) // codes as defined in AMS Streaming Endpoint hostname - to be completed
                {
                    case "euno":
                    case "no":
                        return "North Europe";

                    case "euwe":
                    case "we":
                        return "West Europe";

                    default:
                        return _config["Region"];
                }
            }
        }

        public string AzureRegionCode { get; }

        public string LiveIngestAccessToken => _config["LiveIngestAccessToken"];

        public string IrdetoUserName => _config["IrdetoUserName"];

        public string IrdetoPassword => _config["IrdetoPassword"];

        public string IrdetoAccountId => _config["IrdetoAccountId"];

        public string IrdetoSoapService => _config["IrdetoSoapService"];

        public string IrdetoPlayReadyLAURL => _config["IrdetoPlayReadyLAURL"];

        public string IrdetoWidevineLAURL => _config["IrdetoWidevineLAURL"];

        public string IrdetoFairPlayLAURL => _config["IrdetoFairPlayLAURL"];

        public string IrdetoFairPlayCertificateUrl => _config["IrdetoFairPlayCertificateUrl"];

    }
}