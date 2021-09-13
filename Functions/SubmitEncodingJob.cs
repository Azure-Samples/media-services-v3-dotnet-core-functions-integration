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
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Functions
{
    public static class SubmitEncodingJob
    {
        /// <summary>
        /// Data to pass as an input to the function
        /// </summary>
        private class RequestBodyModel
        {
            /// <summary>
            /// Name of the asset to encode.
            /// Mandatory, except if you provide inputUrl.
            /// </summary>
            [JsonProperty("inputAssetName")]
            public string InputAssetName { get; set; }

            /// <summary>
            /// Input Url of the file to encode.
            /// Example : "https://nimbuscdn-nimbuspm.streaming.mediaservices.windows.net/2b533311-b215-4409-80af-529c3e853622/Ignite-short.mp4"
            /// Mandatory, except if you provide inputAssetName.
            /// </summary>
            [JsonProperty("inputUrl")]
            public string InputUrl { get; set; }

            /// <summary>
            /// Name of the transform.
            /// It will be created if it does not exist. In that case, please set the builtInPreset value to provide the Standard Encoder preset to use.
            /// Mandatory.
            /// </summary>
            [JsonProperty("transformName")]
            public string TransformName { get; set; }

            /// <summary>
            /// If transform does not exist, then the function creates it and use the provided built in preset.
            /// Optional.
            /// </summary>
            [JsonProperty("builtInPreset")]
            public string BuiltInPreset { get; set; }

            /// <summary>
            /// Name of the attached storage account to use for the output asset.
            /// Optional.
            /// </summary>
            [JsonProperty("outputAssetStorageAccount")]
            public string OutputAssetStorageAccount { get; set; }
        }

        /// <summary>
        /// Data output by the function
        /// </summary>
        private class AnswerBodyModel
        {
            /// <summary>
            /// Name of the output asset created.
            /// </summary>
            [JsonProperty("outputAssetName")]
            public string OutputAssetName { get; set; }

            /// <summary>
            /// Name of the job created.
            /// </summary>
            [JsonProperty("jobName")]
            public string JobName { get; set; }
        }


        /// <summary>
        /// Function which submits an encoding job
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("SubmitEncodingJob")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("SubmitEncodingJob");
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));

            // Return bad request if input asset name is not passed in
            if (data.InputAssetName == null && data.InputUrl == null)
            {
                return HttpRequest.ResponseBadRequest(req, "Please pass inputAssetName or inputUrl in the request body");
            }

            // Return bad request if input asset name is not passed in
            if (data.TransformName == null)
            {
                return HttpRequest.ResponseBadRequest(req, "Please pass transformName in the request body");
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

            // Creating a unique suffix so that we don't have name collisions if you run the sample
            // multiple times without cleaning up.
            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
            string jobName = $"job-{uniqueness}";
            string outputAssetName = $"output-{uniqueness}";

            Transform transform;
            try
            {
                // Ensure that you have the encoding Transform.  This is really a one time setup operation.
                transform = await TransformUtils.CreateEncodingTransform(client, log, config.ResourceGroup, config.AccountName, data.TransformName, data.BuiltInPreset);
                log.LogInformation("Transform retrieved.");
            }
            catch (Exception e)
            {
                log.LogError("Error when creating/getting the transform.");
                log.LogError($"{e.Message}");
                return HttpRequest.ResponseBadRequest(req, e.Message);
            }

            Asset outputAsset;
            try
            {
                // Output from the job must be written to an Asset, so let's create one
                outputAsset = await AssetUtils.CreateAssetAsync(client, log, config.ResourceGroup, config.AccountName, outputAssetName, data.OutputAssetStorageAccount);
                log.LogInformation($"Output asset '{outputAssetName}' created.");
            }
            catch (Exception e)
            {
                log.LogError("Error when creating the output asset.");
                log.LogError($"{e.Message}");
                return HttpRequest.ResponseBadRequest(req, e.Message);
            }

            // Job input prepration : asset or url
            JobInput jobInput;
            if (data.InputUrl != null)
            {
                jobInput = new JobInputHttp(files: new[] { data.InputUrl });
                log.LogInformation("Input is a Url.");
            }
            else
            {
                jobInput = new JobInputAsset(assetName: data.InputAssetName);
                log.LogInformation($"Input is asset '{data.InputAssetName}'.");
            }

            Job job;
            try
            {
                // Job submission to Azure Media Services
                job = await JobUtils.SubmitJobAsync(
                                           client,
                                           log,
                                           config.ResourceGroup,
                                           config.AccountName,
                                           data.TransformName,
                                           jobName,
                                           jobInput,
                                           outputAssetName
                                           );
                log.LogInformation($"Job '{jobName}' submitted.");
            }
            catch (Exception e)
            {
                log.LogError("Error when submitting the job.");
                log.LogError($"{e.Message}");
                return HttpRequest.ResponseBadRequest(req, e.Message);
            }
                      
            AnswerBodyModel dataOk = new()
            {
                OutputAssetName = outputAsset.Name,
                JobName = job.Name
            };

            return HttpRequest.ResponseOk(req, dataOk);
        }
    }
}
