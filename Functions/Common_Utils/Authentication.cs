// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Management.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace Common_Utils
{
    public class Authentication
    {
        public static readonly string TokenType = "Bearer";

        private static readonly Lazy<TokenCredential> _msiCredential = new Lazy<TokenCredential>(() =>
        {
            // https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            // Using DefaultAzureCredential allows for local dev by setting environment variables for the current user, provided said user
            // has the necessary credentials to perform the operations the MSI of the Function app needs in order to do its work. Including
            // interactive credentials will allow browser-based login when developing locally.

            return new DefaultAzureCredential(includeInteractiveCredentials: true);
            //return new Azure.Identity.DefaultAzureCredential(new DefaultAzureCredentialOptions {ExcludeAzureCliCredential=true, ExcludeAzurePowerShellCredential=true, ExcludeEnvironmentCredential=false,
            //    ExcludeInteractiveBrowserCredential=true, ExcludeManagedIdentityCredential=true, ExcludeSharedTokenCacheCredential=true, ExcludeVisualStudioCodeCredential=true, ExcludeVisualStudioCredential=true });

        });

        /// <summary>
        /// Creates the AzureMediaServicesClient object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The param is of type ConfigWrapper, which reads values from local configuration file.</param>
        /// <returns>A task.</returns>
        // <CreateMediaServicesClientAsync>
        public static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config, ILogger log, bool interactive = false)
        {
            var scopes = new[] { config.ArmAadAudience + "/.default" };

            var tokenCred = _msiCredential.Value;
            var accesTok = await tokenCred.GetTokenAsync(new TokenRequestContext(scopes: scopes), new System.Threading.CancellationToken());
            ServiceClientCredentials credentials = new TokenCredentials(accesTok.Token, TokenType);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId
            };
        }
        // </CreateMediaServicesClientAsync>
    }
}