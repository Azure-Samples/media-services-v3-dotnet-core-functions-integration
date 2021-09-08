// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Common_Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class DeleteAsset
    {
        /// <summary>
        /// Data to pass as an input to the function
        /// </summary>
        private class RequestBodyModel
        {
            /// <summary>
            /// Name of the asset to delete.
            /// Mandatory.
            /// </summary>
            [JsonProperty("assetName")]
            public string AssetName { get; set; }
        }

        /// <summary>
        /// Function which deletes an asset.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("DeleteAsset")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("DeleteAsset");
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));

            // Return bad request if asset name is not passed in
            if (data.AssetName == null)
            {
                return HttpRequest.ResponseBadRequest(req, "Please pass asset name in the request body");
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

                return HttpRequest.ResponseBadRequest(req, e.Message);

            }

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            try
            {
                // let's delete the asset
                await client.Assets.DeleteAsync(config.ResourceGroup, config.AccountName, data.AssetName);
                log.LogInformation($"Asset '{data.AssetName}' deleted.");
            }
            catch (Exception e)
            {
                log.LogError("Error when deleting the asset.");
                log.LogError($"{e.Message}");
                return HttpRequest.ResponseBadRequest(req, e.Message);
            }

            return HttpRequest.ResponseOk(req, null);
        }
    }
}
