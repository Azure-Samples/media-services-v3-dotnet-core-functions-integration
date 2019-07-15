//
// Azure Media Services REST API v3 Functions
//
// generate-ims-manifest - this function generates a simple server-side ISM manifest
/*
This function generates a manifest(.ism) from the MP4/M4A files in the asset.It makes this file primary.
This manifest is needed to stream MP4 file(s) with Azure Media Services.

Caution : such assets are not guaranteed to work with Dynamic Packaging.


Note : if you don't pass a sempahore JSON then this function makes guesses to determine the files for the video tracks and audio tracks.
These guesses can be wrong. Please check the SMIL generated data for your scenario and your source assets.


As an option, this function can check that the streaming endpoint returns a successful client manifest.

```c#
Input :
{
    "assetName":"asset-fedtsdslkjdsd",

    "semaphore" : // json structure from model VodSemaphore. If specified, then the function does not do any guess but build the manifest based on the data from the semaphore file
 
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are specified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, the live event will be deleted from the two regions.
            // Note: the service principal must work with all this accounts

    "fileName" : "manifest.ism", // Optional. file name of the manifest to create

}

Output:
{
    "success": true,
    "errorMessage" : "",
    "operationsVersion": "1.0.0.5",
     "fileName" : "manifest.ism" // The name of the manifest file created
    "manifestContent" : "" // the SMIL data created as an asset file 
}




Example of generated manifest file :
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<smil xmlns="http://www.w3.org/2001/SMIL20/Language">
  <head>
    <meta name="formats" content="mp4" />
  </head>
  <body>
    <switch>
      <audio src="625290641000.mp4" systemLanguage="fra">
        <param name="trackName" value="french-audio" />
      </audio>
      <audio src="gsw_CPL.mp4" systemLanguage="fra">
        <param name="trackName" value="french-audio-description" />
        <param name="accessibility" value="description" />
        <param name="role" value="alternate" />
      </audio>
      <video src="625290641000.mp4" />
    </switch>
  </body>
</smil>


```
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using LiveDRMOperationsV3.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LiveDRMOperationsV3.Models;


namespace LiveDrmOperationsV3
{
    public static class GenerateIsm
    {
        [FunctionName("generate-ism-manifest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext execContext)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var generalOutputInfos = new List<GeneralOutputInfo>();

            var assetName = (string)data.assetName;
            if (assetName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass assetName in the JSON");

            var fileName = (string)data.fileName;
            ManifestGenerated smildata = null;

            VodSemaphore semaphore = null;
            if (data.semaphore != null)
            {
                semaphore = VodSemaphore.FromJson((string)data.semaphore);
            }

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

                MediaServicesHelpers.LogInformation(log, "asset name : " + assetName, region);
                var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, assetName);

                if (asset == null)
                    throw new Exception($"Asset {asset}  does not exist.");

                // Access to container
                ListContainerSasInput input = new ListContainerSasInput()
                {
                    Permissions = AssetContainerPermission.ReadWriteDelete,
                    ExpiryTime = DateTime.Now.AddHours(2).ToUniversalTime()
                };

                var responseListSas = await client.Assets.ListContainerSasAsync(config.ResourceGroup, config.AccountName, asset.Name, input.Permissions, input.ExpiryTime);

                string uploadSasUrl = responseListSas.AssetContainerSasUrls.First();

                var sasUri = new Uri(uploadSasUrl);
                var container = new CloudBlobContainer(sasUri);

                // Manifest generate
                smildata = (semaphore == null) ? await ManifestHelpers.LoadAndUpdateManifestTemplate(asset, container, execContext, fileName)
                                                : await ManifestHelpers.LoadAndUpdateManifestTemplateUsingSemaphore(semaphore, execContext, fileName);
                // if not file name passed, then we use the one generated based on mp4 files names
                if (fileName == null)
                {
                    fileName = smildata.FileName;
                }

                var blob = container.GetBlockBlobReference(fileName);

                using (Stream s = ManifestHelpers.GenerateStreamFromString(smildata.Content))
                {
                    await blob.UploadFromStreamAsync(s);
                }
            }

            var response = new JObject
            {
                {"success", true},
                {"fileName",  fileName},
                {"manifestContent", smildata.Content },
                {
                    "operationsVersion",
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                }
            };

            return new OkObjectResult(
                response
            );
        }
    }
}
