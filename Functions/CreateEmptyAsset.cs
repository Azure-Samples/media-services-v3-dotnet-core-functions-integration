// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Common_Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class CreateEmptyAsset
    {
        /// <summary>
        /// Data to pass as an input to the function
        /// </summary>
        private class RequestBodyModel
        {
            /// <summary>
            /// Prefix of the name of the asset to create. Function will add some uniqueness.
            /// Mandatory.
            /// </summary>
            [JsonProperty("assetNamePrefix")]
            public string AssetNamePrefix { get; set; }

            /// <summary>
            /// Asset description
            /// Optional.
            /// </summary>
            [JsonProperty("assetDescription")]
            public string AssetDescription { get; set; }

            /// <summary>
            /// Name of the attached storage account to use for the asset.
            /// Optional.
            /// </summary>
            [JsonProperty("assetStorageAccount")]
            public string AssetStorageAccount { get; set; }
        }

        /// <summary>
        /// Data output by the function
        /// </summary>
        private class AnswerBodyModel
        {
            /// <summary>
            /// Name of the asset created.
            /// </summary>
            [JsonProperty("assetName")]
            public string AssetName { get; set; }

            /// <summary>
            /// Id of the asset.
            /// </summary>
            [JsonProperty("assetId")]
            public Guid AssetId { get; set; }

            /// <summary>
            /// Name of the storage container.
            /// </summary>
            [JsonProperty("container")]
            public string Container { get; set; }
        }


        /// <summary>
        /// Function which creates a new asset.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("CreateEmptyAsset")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("CreateEmptyAsset");
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));
            
            // Return bad request if input asset name is not passed in
            if (data.AssetNamePrefix == null)
            {
                return new BadRequestObjectResult("Please pass asset name prefix in the request body");
            }

            ConfigWrapper config = ConfigUtils.GetConfig();

            IAzureMediaServicesClient client;
            try
            {
                client = await Authentication.CreateMediaServicesClientAsync(config, log);
                log.LogInformation("AMS Client created.");
            }
            catch (Exception e)
            {
                if (e.Source.Contains("ActiveDirectory"))
                {
                    log.LogError("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                }
                log.LogError($"{e.Message}");

                return new BadRequestObjectResult(e.Message);
            }

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;
            
            // Creating a unique suffix so that we don't have name collisions if you run the sample
            // multiple times without cleaning up.
            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            string assetName = $"{data.AssetNamePrefix}-{uniqueness}";

            Asset asset;
            try
            {
                // let's create the asset
                asset = await AssetUtils.CreateAssetAsync(client, log, config.ResourceGroup, config.AccountName, assetName, data.AssetStorageAccount);
                log.LogInformation($"Asset '{assetName}' created.");

                // let's get the asset to have full metadata like container
                asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName, assetName);
            }
            catch (Exception e)
            {
                log.LogError("Error when creating the asset.");
                log.LogError($"{e.Message}");
                return new BadRequestObjectResult(e.Message);
            }

            return new OkObjectResult(new AnswerBodyModel
            {
                AssetName = asset.Name,
                AssetId = asset.AssetId,
                Container = asset.Container
            });
        }
    }
}