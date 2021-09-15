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
            /// Streaming locator Id.
            /// For example "911b65de-ac92-4391-9aab-80021126d403"
            /// Optional.
            /// </summary>
            [JsonProperty("streamingLocatorId")]
            public string StreamingLocatorId { get; set; }

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

            /// <summary>
            /// JSON string with the content keys to be used by the streaming locator.
            /// Use @{url} to load from a file from the specified URL.
            /// For further information about the JSON structure please refer to swagger documentation on
            /// https://docs.microsoft.com/en-us/rest/api/media/streaminglocators/create#streaminglocatorcontentkey.
            /// Optional.
            /// </summary>
            [JsonProperty("contentKeys")]
            public string ContentKeys { get; set; }
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
        /// Function which publishes an asset.
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
                return HttpRequest.ResponseBadRequest(req, "Please pass assetName in the request body");
            }
            if (data.StreamingPolicyName == null)
            {
                return HttpRequest.ResponseBadRequest(req, "Please pass streamingPolicyName in the request body");
            }

            ConfigWrapper config = ConfigUtils.GetConfig();

            IAzureMediaServicesClient client;
            try
            {
                client = await Authentication.CreateMediaServicesClientAsync(config);
                log.LogInformation("AMS Client created.");
            }
            catch (Exception e)
            {
                if (e.Source.Contains("ActiveDirectory"))
                {
                    log.LogError("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                }
                log.LogError($"{e.Message}");

                return HttpRequest.ResponseBadRequest(req, e.Message);
            }

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            // Creating a unique suffix so that we don't have name collisions if you run the sample
            // multiple times without cleaning up.
            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);

            List<StreamingLocatorContentKey> contentKeys = new List<StreamingLocatorContentKey>();
          
            Guid streamingLocatorId = Guid.NewGuid();
            if (data.StreamingLocatorId != null)
                streamingLocatorId = new Guid((string)(data.StreamingLocatorId));
            string streamingLocatorName = "streaminglocator-" + streamingLocatorId.ToString();

            StreamingPolicy streamingPolicy;
            Asset asset;

            try
            {
                asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName, data.AssetName);
                if (asset == null)
                {
                    return HttpRequest.ResponseBadRequest(req, "Asset not found");
                }

                streamingPolicy = await client.StreamingPolicies.GetAsync(config.ResourceGroup, config.AccountName, data.StreamingPolicyName);
                if (streamingPolicy == null)
                {
                    return HttpRequest.ResponseBadRequest(req, "Streaming Policy not found");
                }

                if (data.ContentKeyPolicyName != null)
                {
                    ContentKeyPolicy contentKeyPolicy = null;
                    contentKeyPolicy = await client.ContentKeyPolicies.GetAsync(config.ResourceGroup, config.AccountName, data.ContentKeyPolicyName);
                    if (contentKeyPolicy == null)
                    {
                        return HttpRequest.ResponseBadRequest(req, "Content Key Policy not found");
                    }
                }

                if (data.ContentKeys != null)
                {
                    JsonConverter[] jsonConverters = {
                        new MediaServicesHelperJsonReader()
                    };
                    contentKeys = JsonConvert.DeserializeObject<List<StreamingLocatorContentKey>>(data.ContentKeys.ToString(), jsonConverters);
                }

                var streamingLocator = new StreamingLocator()
                {
                    AssetName = data.AssetName,
                    StreamingPolicyName = data.StreamingPolicyName,
                    DefaultContentKeyPolicyName = data.ContentKeyPolicyName,
                    StreamingLocatorId = streamingLocatorId,
                    StartTime = data.StartDateTime,
                    EndTime=data.EndDateTime
                };

                if (contentKeys.Count != 0)
                    streamingLocator.ContentKeys = contentKeys;
                streamingLocator.Validate();

                await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, streamingLocatorName, streamingLocator);
            }
            catch (Exception e)
            {
                log.LogError("Error when publishing the asset.");
                log.LogError($"{e.Message}");
                return HttpRequest.ResponseBadRequest(req, e.Message);
            }

            AnswerBodyModel dataOk = new()
            {
                StreamingLocatorName = streamingLocatorName,
                StreamingLocatorId = streamingLocatorId
            };

            return HttpRequest.ResponseOk(req, dataOk);
        }
    }
}
