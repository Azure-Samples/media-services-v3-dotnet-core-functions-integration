//
// Azure Media Services REST API v2 Functions
//
// SubmitMediaJob - This function submits media job.
//
/*
```c#
Input:
    {
        // [Required] The name of the Asset for media job input
        "inputAssetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",

        // [Required] The name of the Transform for media job
        "transformName": "TestTransform",

        // [Required] The name of the Assets for media job outputs
        "outputAssetNamePrefix": "TestOutputAssetName",

        // The name of attached storage account where to create the Output Assets
        "assetStorageAccount": "storage01"
    }
Output:
    {
        // The name of media Job
        "jobName": "amsv3function-job-24369d2e-7415-4ff5-ba12-b8a879a15401",

        // The name of Encdoer Output Asset
        "encoderOutputAssetName": "out-testasset-e389de79-3aa5-4a5a-a9ca-2a6fd8c53968",

        // The name of Video Analyzer Output Asset
        "videoAnalyzerOutputAssetName": "out-testasset-00cd363b-5fe0-4da1-acf8-ebd66ef14504"
    }

```
*/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LiveDrmOperationsV3
{
    public static class submitjob
    {
        [FunctionName("submit-job")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.inputAssetName == null)
                return new BadRequestObjectResult("Please pass inputAssetName in the input object");
            if (data.transformName == null)
                return new BadRequestObjectResult("Please pass transformName in the input object");
            if (data.outputAssetNamePrefix == null)
                return new BadRequestObjectResult("Please pass outputAssetNamePrefix in the input object");
            string inputAssetName = data.inputAssetName;
            string transformName = data.transformName;
            string outputAssetNamePrefix = data.outputAssetNamePrefix;
            string assetStorageAccount = null;
            if (data.assetStorageAccount != null)
                assetStorageAccount = data.assetStorageAccount;

            // Azure region management
            var azureRegions = new List<string>();
            if ((string)data.azureRegion != null)
            {
                azureRegions = ((string)data.azureRegion).Split(',').ToList();
            }
            else
            {
                azureRegions.Add((string)null);
            }

            Asset inputAsset = null;

            string guid = Guid.NewGuid().ToString();
            string jobName = "amsv3function-job-" + guid;
            string encoderOutputAssetName = null;
            string audioAnalyzerOutputAssetName = null;
            string videoAnalyzerOutputAssetName = null;
            JArray outputs = new JArray();

            foreach (var region in azureRegions)
            {
                ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .Build(),
                        region
                );

                MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                // Set the polling interval for long running operations to 2 seconds.
                // The default value is 30 seconds for the .NET client SDK
                client.LongRunningOperationRetryTimeout = 2;

                try
                {
                    inputAsset = client.Assets.Get(config.ResourceGroup, config.AccountName, inputAssetName);
                    if (inputAsset == null)
                        return new BadRequestObjectResult("Asset for input not found");
                    Transform transform = client.Transforms.Get(config.ResourceGroup, config.AccountName, transformName);
                    if (transform == null)
                        return new BadRequestObjectResult("Transform not found");

                    var jobOutputList = new List<JobOutput>();
                    for (int i = 0; i < transform.Outputs.Count; i++)
                    {
                        Guid assetGuid = Guid.NewGuid();
                        string outputAssetName = outputAssetNamePrefix + "-" + assetGuid.ToString();
                        Asset assetParams = new Asset(null, outputAssetName, null, assetGuid, DateTime.Now, DateTime.Now, null, outputAssetName, null, assetStorageAccount, AssetStorageEncryptionFormat.None);
                        Asset outputAsset = client.Assets.CreateOrUpdate(config.ResourceGroup, config.AccountName, outputAssetName, assetParams);
                        jobOutputList.Add(new JobOutputAsset(outputAssetName));

                        Preset preset = transform.Outputs[i].Preset;
                        if (encoderOutputAssetName == null && (preset is BuiltInStandardEncoderPreset || preset is StandardEncoderPreset))
                            encoderOutputAssetName = outputAssetName;
                        if (audioAnalyzerOutputAssetName == null && preset is AudioAnalyzerPreset)
                            audioAnalyzerOutputAssetName = outputAssetName;
                        if (videoAnalyzerOutputAssetName == null && preset is VideoAnalyzerPreset)
                            videoAnalyzerOutputAssetName = outputAssetName;

                        // set up jobOutputs
                        JObject outObject = new JObject();
                        outObject["Preset"] = preset.ToString().Split('.').Last<string>();
                        outObject["OutputAssetName"] = outputAssetName;
                        if (preset is BuiltInStandardEncoderPreset || preset is StandardEncoderPreset)
                            outObject["Category"] = "Encoder";
                        else if (preset is AudioAnalyzerPreset || preset is VideoAnalyzerPreset)
                            outObject["Category"] = "Analyzer";
                        else
                            outObject["Category"] = "Unknown";
                        outputs.Add(outObject);
                    }

                    // Use the name of the created input asset to create the job input.
                    JobInput jobInput = new JobInputAsset(assetName: inputAssetName);
                    JobOutput[] jobOutputs = jobOutputList.ToArray();
                    Job job = client.Jobs.Create(
                        config.ResourceGroup,
                        config.AccountName,
                        transformName,
                        jobName,
                        new Job
                        {
                            Input = jobInput,
                            Outputs = jobOutputs,
                        }
                    );
                }
                catch (ApiErrorException e)
                {
                    log.LogError($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                    return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
                }
                catch (Exception e)
                {
                    log.LogError($"ERROR: Exception with message: {e.Message}");
                    return new BadRequestObjectResult("Error: " + e.Message);
                }
            }


            JObject result = new JObject();
            result["jobName"] = jobName;
            result["jobOutputs"] = outputs;
            result["encoderOutputAssetName"] = encoderOutputAssetName;
            result["videoAnalyzerOutputAssetName"] = videoAnalyzerOutputAssetName;
            result["audioAnalyzerOutputAssetName"] = audioAnalyzerOutputAssetName;
            return (ActionResult)new OkObjectResult(result);
        }
    }
}
