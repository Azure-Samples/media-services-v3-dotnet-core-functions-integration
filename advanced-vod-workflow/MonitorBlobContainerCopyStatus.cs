//
// Azure Media Services REST API v3 - Functions
//
// MonitorBlobContainerCopyStatus - This function monitors blob copy.
//
/*
```c#
Input:
	{
		// Name of the asset for copy destination
		"assetName": "TestAssetName-180c777b-cd3c-4e02-b362-39b8d94d7a85",
		// (Optional) File names for monitoring 
		//      all blobs in the destination container will be monitored if no fileNames
		"fileNames": [ "filename.mp4" , "filename2.mp4" ]
	}
Output:
	{
		// BlobCopy Action Status: true or false
		"copyStatus": true|false,
		// CopyStatus for each blob
		"blobCopyStatusList": [
			{
				// Name of blob
				"blobName": "filename.mp4",
				// Blob CopyStatus code (see below)
				"blobCopyStatus": 2
			}
		]
	}

```
*/
//      // https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.storage.blob.copystatus?view=azure-dotnet
//      //      Invalid     0	The copy status is invalid.
//      //      Pending     1	The copy operation is pending.
//      //      Success     2	The copy operation succeeded.
//      //      Aborted     3	The copy operation has been aborted.
//      //      Failed      4	The copy operation encountered an error.
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


namespace advanced_vod_functions_v3
{
	public static class MonitorBlobContainerCopyStatus
	{
		[FunctionName("MonitorBlobContainerCopyStatus")]
		public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
		{
			log.Info($"AMS v3 Function - MonitorBlobContainerCopyStatus was triggered!");

			string requestBody = new StreamReader(req.Body).ReadToEnd();
			dynamic data = JsonConvert.DeserializeObject(requestBody);

			// Validate input objects
			if (data.assetName == null)
				return new BadRequestObjectResult("Please pass assetName in the input object");
			string assetName = data.assetName;
			List<string> fileNames = null;
			if (data.fileNames != null)
			{
				fileNames = ((JArray)data.fileNames).ToObject<List<string>>();
			}

			bool copyStatus = true;
			JArray blobCopyStatusList = new JArray();

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
				var response = client.Assets.ListContainerSas(amsconfig.ResourceGroup, amsconfig.AccountName, assetName, permissions: AssetContainerPermission.Read, expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
				var sasUri = new Uri(response.AssetContainerSasUrls.First());
				CloudBlobContainer destinationBlobContainer = new CloudBlobContainer(sasUri);

				//string blobPrefix = null;
				//bool useFlatBlobListing = true;
				//BlobContinuationToken blobContinuationToken = null;

				log.Info("Checking CopyStatus of all blobs in the source container...");
				var blobList = BlobStorageHelper.ListBlobs(destinationBlobContainer);
				foreach (var blob in blobList)
				{
					if (fileNames != null)
					{
						bool found = false;
						foreach (var fileName in fileNames)
						{
							if (fileName == blob.Name)
							{
								found = true;
								break;
							}
						}
						if (found == false) break;
					}

					if (blob.CopyState.Status == CopyStatus.Aborted || blob.CopyState.Status == CopyStatus.Failed)
					{
						// Log the copy status description for diagnostics and restart copy
						blob.StartCopyAsync(blob.CopyState.Source);
						copyStatus = false;
					}
					else if (blob.CopyState.Status == CopyStatus.Pending)
					{
						// We need to continue waiting for this pending copy
						// However, let us log copy state for diagnostics
						copyStatus = false;
					}
					// else we completed this pending copy

					string blobName = blob.Name as string;
					int blobCopyStatus = (int)(blob.CopyState.Status);
					JObject o = new JObject();
					o["blobName"] = blobName;
					o["blobCopyStatus"] = blobCopyStatus;
					blobCopyStatusList.Add(o);
				}
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

			JObject result = new JObject();
			result["copyStatus"] = copyStatus;
			result["blobCopyStatusList"] = blobCopyStatusList;

			return (ActionResult)new OkObjectResult(result);
		}
	}
}
