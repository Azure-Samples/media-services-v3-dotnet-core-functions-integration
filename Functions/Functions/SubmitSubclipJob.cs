using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common_Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace Functions
{
    public static class SubmitSubclipJob
    {
        private class RequestBodyModel
        {
            [JsonProperty("liveEventName")]
            public string LiveEventName { get; set; }

            [JsonProperty("liveOutputName")]
            public string LiveOutputName { get; set; }

            [System.Text.Json.Serialization.JsonConverterAttribute(typeof(TimeSpanConverter))]
            [JsonProperty("lastSubclipEndTime")]
            public TimeSpan? LastSubclipEndTime { get; set; }

            [JsonProperty("outputAssetStorageAccount")]
            public string OutputAssetStorageAccount { get; set; }

            [JsonProperty("intervalSec")]
            public int? IntervalSec { get; set; }
        }

        private class AnswerBodyModel
        {
            [JsonProperty("subclipAssetName")]
            public string SubclipAssetName { get; set; }

            [JsonProperty("subclipJobName")]
            public string SubclipJobName { get; set; }

            [JsonProperty("subclipTransformName")]
            public string SubclipTransformName { get; set; }

            [System.Text.Json.Serialization.JsonConverterAttribute(typeof(TimeSpanConverter))]
            [JsonProperty("subclipEndTime")]
            public TimeSpan SubclipEndTime { get; set; }


        }

        private const string SubclipTransformName = "FunctionSubclipTransform";

        [Function("SubmitSubclipJob")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("Function1");
            log.LogInformation("C# HTTP trigger function processed a request.");

            string triggerStart = DateTime.UtcNow.ToString("yyMMddHHmmss");

            // Get request body data.
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = (RequestBodyModel)JsonConvert.DeserializeObject(requestBody, typeof(RequestBodyModel));

            // Return bad request if input asset name is not passed in
            if (data.LiveEventName == null || data.LiveOutputName == null)
            {
                return new BadRequestObjectResult("Please pass live event name and live output name in the request body");
            }

            data.IntervalSec ??= 60;

            // If Visual Studio is used, let's read the .env file which should be in the root folder (same folder than the solution .sln file).
            // Same code will work in VS Code, but VS Code uses also launch.json to get the .env file.
            // You can create this ".env" file by saving the "sample.env" file as ".env" file and fill it with the right values.
            try
            {
                DotEnv.Load(".env");
            }
            catch
            {

            }

            ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables() // parses the values from the optional .env file at the solution root
                .Build());

            IAzureMediaServicesClient client;
            try
            {
                client = await Authentication.CreateMediaServicesClientAsync(config);
            }
            catch (Exception e)
            {
                if (e.Source.Contains("ActiveDirectory"))
                {
                    Console.Error.WriteLine("TIP: Make sure that you have filled out the appsettings.json file before running this sample.");
                    Console.Error.WriteLine();
                }
                Console.Error.WriteLine($"{e.Message}");

                return new BadRequestObjectResult(e.Message);
            }

            // Set the polling interval for long running operations to 2 seconds.
            // The default value is 30 seconds for the .NET client SDK
            client.LongRunningOperationRetryTimeout = 2;

            // Ensure that you have customized encoding Transform.  This is really a one time setup operation.
            Transform transform = await CreateSubclipTransform(client, config.ResourceGroup, config.AccountName, SubclipTransformName);

            var liveOutput = await client.LiveOutputs.GetAsync(config.ResourceGroup, config.AccountName, data.LiveEventName, data.LiveOutputName);


            // let's analyze the client manifest and adjust times for the subclip job
            var doc = await LiveManifest.TryToGetClientManifestContentAsABlobAsync(client, config.ResourceGroup, config.AccountName, liveOutput.AssetName);
            var assetmanifestdata = LiveManifest.GetManifestTimingData(doc);

            if (assetmanifestdata.Error)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Data cannot be read from live output / asset manifest."
                });
            }

            log.LogInformation("Timestamps : " + string.Join(",", assetmanifestdata.TimestampList.Select(n => n.ToString()).ToArray()));

            var livetime = TimeSpan.FromSeconds(assetmanifestdata.TimestampEndLastChunk / (double)assetmanifestdata.TimeScale);

            log.LogInformation($"Livetime : {livetime}");

            var starttime = LiveManifest.ReturnTimeSpanOnGOP(assetmanifestdata, livetime.Subtract(TimeSpan.FromSeconds((int)data.IntervalSec)));
            log.LogInformation($"Value starttime : {starttime}");

            if (data.LastSubclipEndTime != null)
            {
                var lastEndTime = (TimeSpan)data.LastSubclipEndTime;
                log.LogInformation($"Value lastEndTime : {lastEndTime}");

                var delta = (livetime - lastEndTime - TimeSpan.FromSeconds((int)data.IntervalSec)).Duration();
                log.LogInformation($"Delta: {delta}");

                if (delta < (TimeSpan.FromSeconds(3 * (int)data.IntervalSec))) // less than 3 times the normal duration (3*60s)
                {
                    starttime = lastEndTime;
                    log.LogInformation($"Value new starttime : {starttime}");
                }
            }

            var duration = livetime - starttime;
            log.LogInformation($"Value duration: {duration}");
            if (duration == new TimeSpan(0)) // Duration is zero, this may happen sometimes !
            {
                return new NotFoundObjectResult(new
                {
                    message = "Stopping. Duration of subclip is zero."
                });
            }

            // Output from the Job must be written to an Asset, so let's create one
            Asset outputAsset = await CreateOutputAssetAsync(client, config.ResourceGroup, config.AccountName, liveOutput.Name + "-subclip-" + triggerStart, data.OutputAssetStorageAccount);

            Job job = await SubmitJobAsync(
                client,
                config.ResourceGroup,
                config.AccountName,
                SubclipTransformName,
                $"Subclip-{liveOutput.Name}-{triggerStart}",
                liveOutput.AssetName,
                outputAsset.Name,
                new AbsoluteClipTime(starttime.Subtract(TimeSpan.FromMilliseconds(100))),
                new AbsoluteClipTime(livetime.Add(TimeSpan.FromMilliseconds(100)))
                );


            return new OkObjectResult(new AnswerBodyModel
            {
                SubclipAssetName = outputAsset.Name,
                SubclipJobName = job.Name,
                SubclipTransformName = SubclipTransformName,
                SubclipEndTime = starttime + duration
            });
        }


        /// <summary>
        /// If the specified transform exists, return that transform. If the it does not
        /// exist, creates a new transform with the specified output. In this case, the
        /// output is set to encode a video using a custom preset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The transform name.</param>
        /// <returns></returns>
        private static async Task<Transform> CreateSubclipTransform(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName)
        {
            // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = client.Transforms.Get(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                Console.WriteLine("Creating a custom transform...");
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                  new TransformOutput(
                        CopyOnlyPreset(),
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = "A subclip transform with top bitrate archiving";
                // Create the custom Transform with the outputs defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, outputs, description);
            }

            return transform;
        }


        /// <summary>
        /// Creates an output asset. The output from the encoding Job must be written to an Asset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="assetName">The output asset name.</param>
        /// <returns></returns>
        private static async Task<Asset> CreateOutputAssetAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string assetName, string storageAccountName = null)
        {
            // Check if an Asset already exists
            Asset outputAsset = await client.Assets.GetAsync(resourceGroupName, accountName, assetName);

            if (outputAsset != null)
            {
                // The asset already exists and we are going to overwrite it. In your application, if you don't want to overwrite
                // an existing asset, use an unique name.
                Console.WriteLine($"Warning: The asset named {assetName} already exists. It will be overwritten in this sample.");
            }
            else
            {
                Console.WriteLine("Creating an output asset..");
                outputAsset = new Asset(storageAccountName: storageAccountName);
            }

            return await client.Assets.CreateOrUpdateAsync(resourceGroupName, accountName, assetName, outputAsset);
        }


        /// <summary>
        /// Submits a request to Media Services to apply the specified Transform to a given input video.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The name of the transform.</param>
        /// <param name="jobName">The (unique) name of the job.</param>
        /// <param name="inputAssetName"></param>
        /// <param name="outputAssetName">The (unique) name of the  output asset that will store the result of the encoding job. </param>
        private static async Task<Job> SubmitJobAsync(
            IAzureMediaServicesClient client,
            string resourceGroupName,
            string accountName,
            string transformName,
            string jobName,
            string inputAssetName,
            string outputAssetName,
            ClipTime start = null,
            ClipTime end = null
            )
        {
            JobInput jobInput = new JobInputAsset(assetName: inputAssetName, start: start, end: end);

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
                Console.WriteLine("Creating a job...");
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
            catch (Exception exception)
            {
                if (exception.GetBaseException() is ApiErrorException apiException)
                {
                    Console.Error.WriteLine(
                        $"ERROR: API call failed with error code '{apiException.Body.Error.Code}' and message '{apiException.Body.Error.Message}'.");
                }
                throw;
            }

            return job;
        }


        public static StandardEncoderPreset CopyOnlyPreset()
        {
            return new StandardEncoderPreset(
       codecs: new Codec[]
       {
                        // Add an Audio layer for the audio copy
                        new CopyAudio(),                 
                        // Next, add a Video for the video copy
                       new CopyVideo()
        },
         // Specify the format for the output files - one for video+audio, and another for the thumbnails
         formats: new Format[]
         {
                        new Mp4Format(
                            filenamePattern:"Archive-{Basename}{Extension}"
                        )
         });
        }





    }

    public class TimeSpanConverter : System.Text.Json.Serialization.JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

}
