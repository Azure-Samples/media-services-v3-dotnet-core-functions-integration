// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Common_Utils
{
    public class AssetUtils
    {
        /// <summary>
        /// Creates an asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The asset name.</param>
        /// <param name="storageAccountName">The asset storage name.</param>
        /// <returns></returns>
        public static async Task<Asset> CreateAssetAsync(IAzureMediaServicesClient client, ILogger log, string resourceGroupName, string accountName, string assetName, string storageAccountName = null, string description = null)
        {
            Asset asset;
            try
            {
                // Check if an Asset already exists
                asset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);

                // The asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                // an existing asset, use an unique name.
                log.LogInformation($"Warning: The asset named {assetName} already exists. It will be overwritten by the function.");

            }
            catch (ErrorResponseException ex) when (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                log.LogInformation("Creating an asset...");
                asset = new Asset(storageAccountName: storageAccountName);
            }

            if (description != null)
            {
                asset.Description = description;
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, asset);
        }
    }
}