//
// Azure Media Services REST API v3 - Functions
//
// StartBlobContainerCopyToAsset - This function starts copying blob container to the asset.
//
/*
```c#
Input:
	{
		// Name of the asset for copy destination
		"assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",
		// Id of the asset for copy destination
		"assetId": "nb:cid:UUID:4a5f429c-686c-4f6f-ae86-4078a4e6139e",
		// Name of the storage account for copy source
		"sourceStorageAccountName": "mediaimports",
		// Key of the storage account for copy source
		"sourceStorageAccountKey": "keyxxx==",
		// Blob container name of the storage account for copy source
		"sourceContainer":  "movie-trailer"
		// (Optional) File names of source contents
		//      all blobs in the source container will be copied if no fileNames
		"fileNames": [ "filename.mp4" , "filename2.mp4" ]
	}
Output:
	{
		// Container Name of the asset for copy destination
		"destinationContainer": "asset-4a5f429c-686c-4f6f-ae86-4078a4e6139e"
	}

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions
{
	public static class StartBlobContainerCopyToAsset
	{
		[FunctionName("StartBlobContainerCopyToAsset")]
		public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
		{
			log.Info($"AMS v3 Function - StartBlobContainerCopyToAsset was triggered!");

			string requestBody = new StreamReader(req.Body).ReadToEnd();
			dynamic data = JsonConvert.DeserializeObject(requestBody);

			// Validate input objects
			if (data.assetName == null)
				return new BadRequestObjectResult("Please pass assetName in the input object");
			if (data.sourceStorageAccountName == null)
				return new BadRequestObjectResult("Please pass sourceStorageAccountName in the input object");
			if (data.sourceStorageAccountKey == null)
				return new BadRequestObjectResult("Please pass sourceStorageAccountKey in the input object");
			if (data.sourceContainer == null)
				return new BadRequestObjectResult("Please pass sourceContainer in the input object");
			string assetName = data.assetName;
			string sourceStorageAccountName = data.sourceStorageAccountName;
			string sourceStorageAccountKey = data.sourceStorageAccountKey;
			string sourceContainerName = data.sourceContainer;
			List<string> fileNames = null;
			if (data.fileNames != null)
			{
				fileNames = ((JArray)data.fileNames).ToObject<List<string>>();
			}
			string destinationBlobContainerName = null;

			MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
			Asset asset = null;

			try
			{
				IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

				// Get the Asset
				asset = client.Assets.Get(amsconfig.ResourceGroup, amsconfig.AccountName, assetName);
				if (asset == null)
				{
					return new BadRequestObjectResult("Asset not found");
				}

				// Setup blob container
				CloudBlobContainer sourceBlobContainer = BlobStorageHelper.GetCloudBlobContainer(sourceStorageAccountName, sourceStorageAccountKey, sourceContainerName);
				var response = client.Assets.ListContainerSas(amsconfig.ResourceGroup, amsconfig.AccountName, assetName, permissions: AssetContainerPermission.ReadWrite, expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
				var sasUri = new Uri(response.AssetContainerSasUrls.First());
				CloudBlobContainer destinationBlobContainer = new CloudBlobContainer(sasUri);
				destinationBlobContainerName = destinationBlobContainer.Name;

				// Copy Source Blob container into Destination Blob container that is associated with the asset.
				BlobStorageHelper.CopyBlobsAsync(sourceBlobContainer, destinationBlobContainer, fileNames);
			}
			catch (ApiErrorException e)
			{
				log.Info($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
				return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
			}
			catch (Exception e)
			{
				log.Info($"ERROR: Exception with message: {e.Message}");
				return new BadRequestObjectResult("Error: " + e.Message);
			}

			return (ActionResult)new OkObjectResult(new
			{
				destinationContainer = destinationBlobContainerName
			});
		}
	}
}