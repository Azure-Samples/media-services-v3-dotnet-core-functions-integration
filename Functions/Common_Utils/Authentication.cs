// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.Media;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace Common_Utils
{
    public class Authentication
    {
        public static readonly string TokenType = "Bearer";

        private static readonly Lazy<TokenCredential> _msiCredential = new(() =>
        {
            // https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            // Using DefaultAzureCredential allows for local dev by setting environment variables for the current user, provided said user
            // has the necessary credentials to perform the operations the MSI of the Function app needs in order to do its work. Including
            // interactive credentials will allow browser-based login when developing locally.

            return new DefaultAzureCredential(includeInteractiveCredentials: true);
        });

        /// <summary>
        /// Creates the AzureMediaServicesClient object based on the credentials
        /// supplied in local configuration file or from other types of authentication.
        /// </summary>
        /// <param name="config">The param is of type ConfigWrapper, which reads values from local configuration file.</param>
        /// <returns>A task.</returns>
        // <CreateMediaServicesClientAsync>
        public static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
        {
            string[] scopes = new[] { config.ArmAadAudience + "/.default" };
            TokenCredential tokenCred = _msiCredential.Value;
            AccessToken accesToken = await tokenCred.GetTokenAsync(new TokenRequestContext(scopes), new System.Threading.CancellationToken());
            ServiceClientCredentials credentials = new TokenCredentials(accesToken.Token, TokenType);
            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId
            };
        }
        // </CreateMediaServicesClientAsync>
    }
}