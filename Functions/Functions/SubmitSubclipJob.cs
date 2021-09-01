// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        /// <summary>
        /// Data to pass as an input to the function
        /// </summary>
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

        /// <summary>
        /// Data output by the function
        /// </summary>
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

        /// <summary>
        /// Function which submits a subclipping job for a live output / asset
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("SubmitSubclipJob")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("SubmitSubclipJob");
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

            ConfigWrapper config = new(new ConfigurationBuilder()
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
            Transform transform = await TransformUtils.CreateSubclipTransform(client, config.ResourceGroup, config.AccountName, SubclipTransformName);

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
            Asset outputAsset = await AssetUtils.CreateOutputAssetAsync(client, config.ResourceGroup, config.AccountName, liveOutput.Name + "-subclip-" + triggerStart, data.OutputAssetStorageAccount);

            JobInput jobInput = new JobInputAsset(
                assetName: liveOutput.AssetName,
                start: new AbsoluteClipTime(starttime.Subtract(TimeSpan.FromMilliseconds(100))),
                end: new AbsoluteClipTime(livetime.Add(TimeSpan.FromMilliseconds(100)))
                );

            Job job = await JobUtils.SubmitJobAsync(
               client,
               config.ResourceGroup,
               config.AccountName,
               SubclipTransformName,
               $"Subclip-{liveOutput.Name}-{triggerStart}",
               jobInput,
               outputAsset.Name
               );

            return new OkObjectResult(new AnswerBodyModel
            {
                SubclipAssetName = outputAsset.Name,
                SubclipJobName = job.Name,
                SubclipTransformName = SubclipTransformName,
                SubclipEndTime = starttime + duration
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
