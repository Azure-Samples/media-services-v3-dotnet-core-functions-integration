// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using System;

namespace Common_Utils
{
    /// <summary>
    /// This class reads values from local configuration file resources/conf/appsettings.json.
    /// Please change the configuration using your account information. For more information, see
    /// https://docs.microsoft.com/azure/media-services/latest/access-api-cli-how-to. For security
    /// reasons, do not check in the configuration file to source control.
    /// </summary>
    public class ConfigWrapper
    {
        private readonly IConfiguration _config;

        public ConfigWrapper(IConfiguration config)
        {
            _config = config;
        }

        public string SubscriptionId
        {
            get { return _config["AZURE_SUBSCRIPTION_ID"]; }
        }

        public string ResourceGroup
        {
            get { return _config["AZURE_RESOURCE_GROUP"]; }
        }

        public string AccountName
        {
            get { return _config["AZURE_MEDIA_SERVICES_ACCOUNT_NAME"]; }
        }

        public string AadTenantId
        {
            get { return _config["AZURE_TENANT_ID"]; }
        }

        public string AadClientId
        {
            get { return _config["AZURE_CLIENT_ID"]; }
        }

        public string AadSecret
        {
            get { return _config["AZURE_CLIENT_SECRET"]; }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(_config["AZURE_ARM_TOKEN_AUDIENCE"]); }
        }

        public Uri AadEndpoint
        {
            get { return new Uri(_config["AZURE_AAD_ENDPOINT"]); }
        }

        public Uri ArmEndpoint
        {
            get { return new Uri(_config["AZURE_ARM_ENDPOINT"]); }
        }
    }
}





