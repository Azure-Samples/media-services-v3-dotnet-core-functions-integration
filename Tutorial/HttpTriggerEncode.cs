// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Rest;
using System.Net;
using Newtonsoft.Json.Linq;
using Azure.Identity;
using Azure.Core;
using Microsoft.Identity.Client;

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
        // <Run>
        [Function("HttpTriggerEncode")]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("SubmitEncodingJob");
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));

            // Return bad request if input asset name is not passed in
            if (data.InputAssetName == null && data.InputUrl == null)
            {
                return ResponseBadRequest(req, "Please pass asset name or input Url in the request body");
            }

            // Return bad request if input asset name is not passed in
            if (data.TransformName == null)
            {
                return ResponseBadRequest(req, "Please pass the transform name in the request body");
            }

            ConfigWrapper config = new(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() // parses the values from the optional .env file at the solution root
                .Build());

            IAzureMediaServicesClient client;
            try
            {
                client = await CreateMediaServicesClientAsync(config);
                log.LogInformation("AMS Client created.");
            }
            catch (Exception e)
            {
                if (e.Source.Contains("ActiveDirectory"))
                {
                    log.LogError("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                }
                log.LogError($"{e.Message}");
                return ResponseBadRequest(req, e.Message);
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
                transform = await CreateEncodingTransform(client, log, config.ResourceGroup, config.AccountName, data.TransformName, data.BuiltInPreset);
                log.LogInformation("Transform retrieved.");
            }
            catch (Exception e)
            {
                log.LogError("Error when creating/getting the transform.");
                log.LogError($"{e.Message}");
                return ResponseBadRequest(req, e.Message);
            }

            Asset outputAsset;
            try
            {
                // Output from the job must be written to an Asset, so let's create one
                outputAsset = await CreateOutputAssetAsync(client, log, config.ResourceGroup, config.AccountName, outputAssetName, data.OutputAssetStorageAccount);
                log.LogInformation($"Output asset '{outputAssetName}' created.");
            }
            catch (Exception e)
            {
                log.LogError("Error when creating the output asset.");
                log.LogError($"{e.Message}");
                return ResponseBadRequest(req, e.Message);
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
                job = await SubmitJobAsync(
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
                return ResponseBadRequest(req, e.Message);
            }

            AnswerBodyModel dataOk = new()
            {
                OutputAssetName = outputAsset.Name,
                JobName = job.Name
            };
            return ResponseOk(req, dataOk);
        }
        // </Run>

        /// <summary>
        /// Generates the response with HttpStatusCode.OK and JSON body
        /// </summary>
        /// <param name="req">HttpRequestData object</param>
        /// <param name="data">Object to serialize</param>
        /// <returns></returns>
        private static HttpResponseData ResponseOk(HttpRequestData req, AnswerBodyModel data)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.WriteString(JsonConvert.SerializeObject(data));
            return response;
        }

        /// <summary>
        /// Generates the response with HttpStatusCode.BadRequest and JSON body
        /// </summary>
        /// <param name="req">HttpRequestData object</param>
        /// <param name="message">Error message</param>
        /// <returns></returns>
        private static HttpResponseData ResponseBadRequest(HttpRequestData req, string message)
        {
            dynamic dataNotOk = new JObject();
            dataNotOk.errorMessage = message;

            var response = req.CreateResponse(HttpStatusCode.BadRequest);
            response.Headers.Add("Content-Type", "application/json");
            var stringJson = JsonConvert.SerializeObject(dataNotOk);
            response.WriteString((string)stringJson);
            return response;
        }

        /// <summary>
        /// Creates the AzureMediaServicesClient object based on the credentials
        /// supplied in local configuration file.
        /// </summary>
        /// <param name="config">The param is of type ConfigWrapper, which reads values from local configuration file.</param>
        /// <returns>A task.</returns>
        private static async Task<IAzureMediaServicesClient> CreateMediaServicesClientAsync(ConfigWrapper config)
        {
            ServiceClientCredentials credentials = await GetCredentialsAsync(config);

            return new AzureMediaServicesClient(config.ArmEndpoint, credentials)
            {
                SubscriptionId = config.SubscriptionId
            };
        }

        private static readonly string TokenType = "Bearer";

        /// <summary>
        /// Create the ServiceClientCredentials object based on the credentials
        /// supplied in local configuration file, or with a system managed identity.
        /// </summary>
        /// <param name="config">The param is of type ConfigWrapper. This class reads values from local configuration file.</param>
        /// <returns></returns>
        private static async Task<ServiceClientCredentials> GetCredentialsAsync(ConfigWrapper config)
        {
            var scopes = new[] { config.ArmAadAudience + "/.default" };

            string token;
            if (config.AadClientId != null) // Service Principal
            {
                var app = ConfidentialClientApplicationBuilder.Create(config.AadClientId)
                .WithClientSecret(config.AadSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, config.AadTenantId)
                .Build();

                var authResult = await app.AcquireTokenForClient(scopes)
                                                        .ExecuteAsync()
                                                        .ConfigureAwait(false);

                token = authResult.AccessToken;
            }
            else // managed identity
            {
                var credential = new ManagedIdentityCredential();
                var accessTokenRequest = await credential.GetTokenAsync(
                    new TokenRequestContext(
                        scopes: scopes
                        )
                    );
                token = accessTokenRequest.Token;
            }
            return new TokenCredentials(token, TokenType);
        }

        /// <summary>
        /// If the specified transform exists, return that transform. If the it does not
        /// exist, creates a new transform with the specified output. In this case, the
        /// output is set to encode a video using a predefined preset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="log">Function logger.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The transform name.</param>
        /// <param name="builtInPreset">The built in standard encoder preset to use if the transform is created.</param>
        /// <returns></returns>
        private static async Task<Transform> CreateEncodingTransform(IAzureMediaServicesClient client, ILogger log, string resourceGroupName, string accountName, string transformName, string builtInPreset)
        {
            bool createTransform = false;
            Transform transform = null;

            try
            {
                // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
                // also uses the same recipe or Preset for processing content.
                transform = client.Transforms.Get(resourceGroupName, accountName, transformName);
            }
            catch (ErrorResponseException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    createTransform = true;
                    log.LogInformation("Transform not found.");
                }
                else
                {
                    throw;
                }
            }

            if (createTransform)
            {
                log.LogInformation($"Creating transform '{transformName}'...");
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                  new TransformOutput(
                        new BuiltInStandardEncoderPreset()
                        {
                            // Pass the buildin preset name.
                            PresetName = builtInPreset
                        },
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = $"An encoding transform using {builtInPreset} preset";

                // Create the Transform with the outputs defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, outputs, description);
            }
            else
            {
                log.LogInformation($"Transform '{transformName}' found in AMS account.");
            }

            return transform;
        }

        /// <summary>
        /// Creates an output asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="log">Function logger.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <param name="storageAccountName">The output asset storage name.</param>
        /// <returns></returns>
        private static async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, ILogger log, string resourceGroupName, string accountName, string assetName, string storageAccountName = null)
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
            catch (ErrorResponseException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    log.LogInformation("Creating an output asset...");
                    asset = new Asset(storageAccountName: storageAccountName);
                }
                else
                {
                    throw;
                }
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, asset);
        }

        /// <summary>
        /// Submits a request to Media Services to apply the specified Transform to a given input video.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="log">Function logger.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The (unique) name of the job.</param>
        /// <param name="jobInput">The input of the job</param>
        /// <param name="outputAssetName">The (unique) name of the  output asset that will store the result of the encoding job. </param>
        private static async Task<Job> SubmitJobAsync(
            IAzureMediaServicesClient client,
            ILogger log,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            JobInput jobInput,
            string outputAssetName
            )
        {
            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            // In this example, we are assuming that the job name is unique.
            //
            // If you already have a job with the desired name, use the Jobs.Get method
            // to get the existing job. In Media Services v3, Get methods on entities returns null 
            // if the entity doesn't exist (a case-insensitive check on the name).
            Job job;
            try
            {
                log.LogInformation("Creating a job...");
                job = await client.Jobs.CreateAsync(
                         resourceGroupName,
                         accountName,
                         transformName,
                         jobName,
                         new Job
                         {
                             Input = jobInput,
                             Outputs = jobOutputs,
                         });
            }
            catch (ErrorResponseException exception)
            {
                log.LogError($"ERROR: API call failed with error code '{exception.Body.Error.Code}' and message '{exception.Body.Error.Message}'.");
                throw;
            }
            return job;
        }
    }

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
            get { return _config["SubscriptionId"]; }
        }
        public string ResourceGroup
        {
            get { return _config["ResourceGroup"]; }
        }
        public string AccountName
        {
            get { return _config["AccountName"]; }
        }
        public string AadTenantId
        {
            get { return _config["AadTenantId"]; }
        }
        public string AadClientId
        {
            get { return _config["AadClientId"]; }
        }
        public string AadSecret
        {
            get { return _config["AadSecret"]; }
        }
        public Uri ArmAadAudience
        {
            get { return new Uri(_config["ArmAadAudience"]); }
        }
        public Uri AadEndpoint
        {
            get { return new Uri(_config["AadEndpoint"]); }
        }
        public Uri ArmEndpoint
        {
            get { return new Uri(_config["ArmEndpoint"]); }
        }
    }
}