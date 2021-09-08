// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
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
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req, FunctionContext executionContext)
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
                return HttpRequest.ResponseBadRequest(req, "Please pass live event name and live output name in the request body");
            }

            data.IntervalSec ??= 60;

            ConfigWrapper config = ConfigUtils.GetConfig();

            IAzureMediaServicesClient client;
            try
            {
                client = await Authentication.CreateMediaServicesClientAsync(config, log);
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

            // Ensure that you have customized encoding Transform.  This is really a one time setup operation.
            Transform transform = await TransformUtils.CreateSubclipTransform(client, log, config.ResourceGroup, config.AccountName, SubclipTransformName);

            var liveOutput = await client.LiveOutputs.GetAsync(config.ResourceGroup, config.AccountName, data.LiveEventName, data.LiveOutputName);


            // let's analyze the client manifest and adjust times for the subclip job
            var doc = await LiveManifest.TryToGetClientManifestContentAsABlobAsync(client, config.ResourceGroup, config.AccountName, liveOutput.AssetName);
            var assetmanifestdata = LiveManifest.GetManifestTimingData(doc);

            if (assetmanifestdata.Error)
            {
                return HttpRequest.ResponseBadRequest(req, "Data cannot be read from live output / asset manifest.");
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
                return HttpRequest.ResponseBadRequest(req, "Stopping. Duration of subclip is zero.");
            }

            // Output from the Job must be written to an Asset, so let's create one
            Asset outputAsset = await AssetUtils.CreateAssetAsync(client, log, config.ResourceGroup, config.AccountName, liveOutput.Name + "-subclip-" + triggerStart, data.OutputAssetStorageAccount);

            JobInput jobInput = new JobInputAsset(
                assetName: liveOutput.AssetName,
                start: new AbsoluteClipTime(starttime.Subtract(TimeSpan.FromMilliseconds(100))),
                end: new AbsoluteClipTime(livetime.Add(TimeSpan.FromMilliseconds(100)))
                );

            Job job = await JobUtils.SubmitJobAsync(
               client,
               log,
               config.ResourceGroup,
               config.AccountName,
               SubclipTransformName,
               $"Subclip-{liveOutput.Name}-{triggerStart}",
               jobInput,
               outputAsset.Name
               );

            AnswerBodyModel dataOk = new()
            {
                SubclipAssetName = outputAsset.Name,
                SubclipJobName = job.Name,
                SubclipTransformName = SubclipTransformName,
                SubclipEndTime = starttime + duration
            };

            return HttpRequest.ResponseOk(req, dataOk);
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
