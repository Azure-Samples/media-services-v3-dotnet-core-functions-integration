using System;
using Microsoft.Extensions.Configuration;

namespace LiveDrmOperationsV3.Helpers
{
    public class ConfigWrapper
    {
        private  readonly IConfiguration _config;
        private readonly string _azureRegionCode;

        public ConfigWrapper(IConfiguration config, string azureRegionCode = null)
        {
            _config = config;
            _azureRegionCode = azureRegionCode;
        }

        public string SubscriptionId
        {
            get { return _config["SubscriptionId"]; }
        }

        public string ResourceGroup
        {
            get { return _config["ResourceGroup"] + _azureRegionCode; }
        }

        public string AccountName
        {
            get { return _config["AccountName"] + _azureRegionCode; }
        }

        public string AadTenantId
        {
            get { return _config["AadTenantId"]; }
        }

        public string AadClientId
        {
            get { return _config["AadClientId"]; }
        }

        public string AadSecret
        {
            get { return _config["AadSecret"]; }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(_config["ArmAadAudience"]); }
        }

        public Uri AadEndpoint
        {
            get { return new Uri(_config["AadEndpoint"]); }
        }

        public Uri ArmEndpoint
        {
            get { return new Uri(_config["ArmEndpoint"]); }
        }

        public string Region
        {
            get
            {

                if (_azureRegionCode == null)
                {
                    return _config["Region"];
                }
                else
                {
                    switch (_azureRegionCode)  // codes as defined in AMS Streaming Endpoint hostname - to be completed
                    {
                        case "euno":
                        case "eu":
                            return "North Europe";

                        case "euwe":
                        case "we":
                            return "West Europe";

                        default:
                            return _config["Region"];

                    }
                }
            }
        }

        public string AzureRegionCode
        {
            get
            {
                return _azureRegionCode;
            }
        }


        public string IrdetoUserName
        {
            get { return _config["IrdetoUserName"]; }
        }
        public string IrdetoPassword
        {
            get { return _config["IrdetoPassword"]; }
        }
        public string IrdetoAccountId
        {
            get { return _config["IrdetoAccountId"]; }
        }
        public string IrdetoSoapService
        {
            get { return _config["IrdetoSoapService"]; }
        }
        public string IrdetoPlayReadyLAURL
        {
            get { return _config["IrdetoPlayReadyLAURL"]; }
        }
        public string IrdetoWidevineLAURL
        {
            get { return _config["IrdetoWidevineLAURL"]; }
        }
        public string IrdetoFairPlayLAURL
        {
            get { return _config["IrdetoFairPlayLAURL"]; }
        }
        public string CosmosConnectionString
        {
            get { return _config["CosmosConnectionString"]; }
        }
        public string CosmosDB
        {
            get { return _config["CosmosDB"]; }
        }
        public string CosmosCollectionOutputs
        {
            get { return _config["CosmosCollectionOutputs"]; }
        }
        public string CosmosCollectionSettings
        {
            get { return _config["CosmosCollectionSettings"]; }
        }
    }
}