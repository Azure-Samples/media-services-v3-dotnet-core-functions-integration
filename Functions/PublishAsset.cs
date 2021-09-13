// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Common_Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class PublishAsset
    {
        /// <summary>
        /// Data to pass as an input to the function
        /// </summary>
        private class RequestBodyModel
        {
            /// <summary>
            /// Name of the asset to publish.
            /// Mandatory.
            /// </summary>
            [JsonProperty("assetName")]
            public string AssetName { get; set; }

            /// <summary>
            /// Streaming policy name
            /// Mandatory.
            /// You can either create one with `CreateStreamingPolicy` or use any of the predefined policies:
            /// Predefined_ClearKey,
            /// Predefined_ClearStreamingOnly,
            /// Predefined_DownloadAndClearStreaming,
            /// Predefined_DownloadOnly,
            /// Predefined_MultiDrmCencStreaming,
            /// Predefined_MultiDrmStreaming.
            /// </summary>
            [JsonProperty("streamingPolicyName")]
            public string StreamingPolicyName { get; set; }

            /// <summary>
            /// Content key policy name
            /// Optional.
            /// </summary>
            [JsonProperty("contentKeyPolicyName")]
            public string ContentKeyPolicyName { get; set; }

            /// <summary>
            /// Start time of the locator
            /// Optional.
            /// </summary>
            [JsonProperty("startDateTime")]
            public DateTime? StartDateTime { get; set; }

            /// <summary>
            /// End time of the locator
            /// Optional.
            /// </summary>
            [JsonProperty("endDateTime")]
            public DateTime? EndDateTime { get; set; }
        }

        /// <summary>
        /// Data output by the function
        /// </summary>
        private class AnswerBodyModel
        {
            /// <summary>
            /// Name of the streaming locator created.
            /// </summary>
            [JsonProperty("streamingLocatorName")]
            public string StreamingLocatorName { get; set; }

            /// <summary>
            /// Id of the locator.
            /// </summary>
            [JsonProperty("streamingLocatorId")]
            public Guid StreamingLocatorId { get; set; }
        }


        /// <summary>
        /// Function which creates a new asset.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("PublishAsset")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("PublishAsset");
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));
            
            // Return bad request if input asset name is not passed in
            if (data.AssetName == null)
            {
                return new BadRequestObjectResult("Please pass asset name in the request body");
            }
            if (data.StreamingPolicyName == null)
            {
                return new BadRequestObjectResult("Please pass the streaming policy name in the request body");
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




            List<StreamingLocatorContentKey> contentKeys = new List<StreamingLocatorContentKey>();
            DateTime startDateTime = new DateTime(0);
            if (data.StartDateTime != null)
                startDateTime = data.startDateTime;
            DateTime endDateTime = new DateTime(0);
            if (data.EndDateTime != null)
                endDateTime = data.endDateTime;
            Guid streamingLocatorId = Guid.NewGuid();
            if (data.s.streamingLocatorId != null)
                streamingLocatorId = new Guid((string)(data.streamingLocatorId));
            string streamingLocatorName = "streaminglocator-" + streamingLocatorId.ToString();


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
