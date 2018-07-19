using System;
using Microsoft.Extensions.Configuration;

namespace ImgDrmOperationsV2
{
    public class ConfigWrapper
    {
        private readonly IConfiguration _config;

        public ConfigWrapper(IConfiguration config)
        {
            _config = config;
        }

        public string SubscriptionId
        {
            get { return _config["SubscriptionId"]; }
        }

        public string ResourceGroup
        {
            get { return _config["ResourceGroup"]; }
        }

        public string AccountName
        {
            get { return _config["AccountName"]; }
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
            get { return _config["Region"]; }
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
       
    }
}