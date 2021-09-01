// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Common_Utils
{
    public class AssetUtils
    {
        /// <summary>
        /// Creates an output asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <param name="storageAccountName">The output asset storage name.</param>
        /// <returns></returns>
        public static async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, ILogger log, string resourceGroupName, string accountName, string assetName, string storageAccountName = null)
        {
            // Check if an Asset already exists
            Asset outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);

            if (outputAsset != null)
            {
                // The asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                // an existing asset, use an unique name.
                log.LogInformation($"Warning: The asset named {assetName} already exists. It will be overwritten by the function.");
            }
            else
            {
                log.LogInformation("Creating an output asset..");
                outputAsset = new Asset(storageAccountName: storageAccountName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, outputAsset);
        }
    }
}