//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;
using System.Collections.Generic;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;


namespace advanced_vod_functions_v3.SharedLibs
{
    public class BlobStorageHelper
    {
        static public CloudBlobContainer GetCloudBlobContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(storageAccountName, storageAccountKey), true);
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
            return cloudBlobClient.GetContainerReference(containerName);
        }

        static public List<CloudBlob> ListBlobs(CloudBlobContainer blobContainer)
        {
            List<CloudBlob> blobList = new List<CloudBlob>();
            string blobPrefix = null;
            bool useFlatBlobListing = true;
            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var results = blobContainer.ListBlobsSegmentedAsync(blobPrefix, useFlatBlobListing, BlobListingDetails.Copy, null, blobContinuationToken, null, null).Result;

                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;

                foreach (IListBlobItem item in results.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        blobList.Add(blob);
                    }
                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null.

            return blobList;
        }

        static public async void CopyBlobsAsync(CloudBlobContainer sourceBlobContainer, CloudBlobContainer destinationBlobContainer, List<string> fileNames)
        {
            if (fileNames != null)
            {
                foreach (var fileName in fileNames)
                {
                    CloudBlob sourceBlob = sourceBlobContainer.GetBlockBlobReference(fileName);
                    CloudBlob destinationBlob = destinationBlobContainer.GetBlockBlobReference(fileName);
                    CopyBlobAsync(sourceBlob as CloudBlob, destinationBlob);
                }
            }
            else
            {
                string blobPrefix = null;
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await sourceBlobContainer.ListBlobsSegmentedAsync(blobPrefix, blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        if (item.GetType() == typeof(CloudBlockBlob))
                        {
                            CloudBlockBlob sourceBlob = (CloudBlockBlob)item;
                            CloudBlob destinationBlob = destinationBlobContainer.GetBlockBlobReference(sourceBlob.Name);
                            CopyBlobAsync(sourceBlob as CloudBlob, destinationBlob);
                        }
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.
            }
        }

        static public async void CopyBlobAsync(CloudBlob sourceBlob, CloudBlob destinationBlob)
        {
            var signature = sourceBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            });
            await destinationBlob.StartCopyAsync(new Uri(sourceBlob.Uri.AbsoluteUri + signature));
        }
    }
}
