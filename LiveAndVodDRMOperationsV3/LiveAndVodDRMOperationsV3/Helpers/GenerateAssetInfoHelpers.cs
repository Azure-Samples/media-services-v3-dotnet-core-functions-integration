using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Models;
using LiveDRMOperationsV3.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace LiveDrmOperationsV3.Helpers
{
    internal partial class GenerateInfoHelpers
    {
        public static async Task<AssetEntry> GenerateAssetInformation(ConfigWrapper config,
            IAzureMediaServicesClient client, Asset asset, VodSemaphore semaphore, string contentId = null, string region = null)
        {
            var assetEntry = new AssetEntry()
            {
                AMSAccountName = config.AccountName,
                Region = config.Region,
                ResourceGroup = config.ResourceGroup,
                AssetStorageAccountName = asset?.StorageAccountName,
                AssetName = asset.Name,
                Urn = semaphore.Urn,
                Semaphore = semaphore,
                StreamingLocators = new List<StreamingLocatorEntry>(),
                CreatedTime = asset.Created.ToUniversalTime().ToString(AssetEntry.DateFormat),
                ContentId = contentId,
                RegionCode = region
            };

            var urls = new List<OutputUrl>();
            string streamingPolicyName = null;
            StreamingPolicy streamingPolicy = null;

            var streamingLocatorsNames = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, asset.Name).StreamingLocators.Select(l => l.Name);
            foreach (var locatorName in streamingLocatorsNames)
            {
                string cenckeyId = null;
                string cbcskeyId = null;
                StreamingLocator streamingLocator = null;

                if (locatorName != null)
                {
                    streamingLocator = client.StreamingLocators.Get(config.ResourceGroup,
                        config.AccountName, locatorName);
                    if (streamingLocator != null)
                    {
                        streamingPolicyName = streamingLocator.StreamingPolicyName;

                        if (streamingLocator.ContentKeys
                                .Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCenc)
                                .FirstOrDefault() != null && streamingLocator.ContentKeys
                                .Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCbcs)
                                .FirstOrDefault() != null)
                        {
                            cenckeyId = streamingLocator.ContentKeys
                                .Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCenc)
                                .FirstOrDefault().Id.ToString();
                            cbcskeyId = streamingLocator.ContentKeys
                                .Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCbcs)
                                .FirstOrDefault().Id.ToString();
                        }


                        // let's get the manifest name
                        string manifestName = null;
                        List<IListBlobItem> blobs = new List<IListBlobItem>();
                        try
                        {
                            ListContainerSasInput input = new ListContainerSasInput()
                            {
                                Permissions = AssetContainerPermission.Read,
                                ExpiryTime = DateTime.Now.AddHours(2).ToUniversalTime()
                            };

                            var responseListSas = client.Assets.ListContainerSas(config.ResourceGroup, config.AccountName, asset.Name, input.Permissions, input.ExpiryTime);
                            string uploadSasUrl = responseListSas.AssetContainerSasUrls.First();

                            var sasUri = new Uri(uploadSasUrl);
                            var container = new CloudBlobContainer(sasUri);
                            var blobsR = await container.ListBlobsSegmentedAsync(null);
                            blobs = blobsR.Results.ToList();
                            // let's take the first manifest file. It should exist
                            manifestName = blobs.Where(b => (b.GetType() == typeof(CloudBlockBlob))).Select(b => (CloudBlockBlob)b).Where(b => b.Name.ToLower().EndsWith(".ism")).FirstOrDefault().Name;
                        }
                        catch
                        {

                        }
                        if (manifestName != null) // there is a manifest
                        {
                            urls = MediaServicesHelpers.GetUrls(config, client, streamingLocator, manifestName, true, true, true, true, true);
                        }
                        else // no manifest
                        {
                            urls = MediaServicesHelpers.GetDownloadUrls(config, client, streamingLocator, blobs);
                        }
                    }
                }

                if (streamingPolicyName != null)
                    streamingPolicy = client.StreamingPolicies.Get(config.ResourceGroup, config.AccountName,
                        streamingPolicyName);

                var drmlist = new List<Drm>();
                if (streamingPolicy != null)
                {
                    if (streamingPolicy.CommonEncryptionCbcs != null)
                    {
                        var enProt =
                            MediaServicesHelpers.ReturnOutputProtocolsListCbcs(streamingPolicy
                                .CommonEncryptionCbcs.EnabledProtocols);
                        drmlist.Add(new Drm
                        {
                            Type = "FairPlay",
                            LicenseUrl =
                                streamingPolicy?.CommonEncryptionCbcs?.Drm.FairPlay
                                    .CustomLicenseAcquisitionUrlTemplate.Replace("{ContentKeyId}", cbcskeyId).Replace("{AlternativeMediaId}", streamingLocator.AlternativeMediaId),
                            Protocols = enProt,
                            CertificateUrl = config.IrdetoFairPlayCertificateUrl
                        });
                    }

                    if (streamingPolicy.CommonEncryptionCenc != null)
                    {
                        var enProtW =
                            MediaServicesHelpers.ReturnOutputProtocolsListCencWidevine(streamingPolicy
                                .CommonEncryptionCbcs.EnabledProtocols);
                        var enProtP =
                            MediaServicesHelpers.ReturnOutputProtocolsListCencPlayReady(streamingPolicy
                                .CommonEncryptionCbcs.EnabledProtocols);

                        drmlist.Add(new Drm
                        {
                            Type = "PlayReady",
                            LicenseUrl = streamingPolicy?.CommonEncryptionCenc?.Drm.PlayReady
                                .CustomLicenseAcquisitionUrlTemplate.Replace("{AlternativeMediaId}", streamingLocator.AlternativeMediaId),
                            Protocols = enProtP
                        });
                        drmlist.Add(new Drm
                        {
                            Type = "Widevine",
                            LicenseUrl = streamingPolicy?.CommonEncryptionCenc?.Drm.Widevine
                                .CustomLicenseAcquisitionUrlTemplate.Replace("{AlternativeMediaId}", streamingLocator.AlternativeMediaId),
                            Protocols = enProtW
                        });
                    }
                }

                var StreamingLocatorInfo = new StreamingLocatorEntry
                {
                    StreamingLocatorName = locatorName,
                    StreamingPolicyName = streamingPolicyName,
                    CencKeyId = cenckeyId,
                    CbcsKeyId = cbcskeyId,
                    Drm = drmlist,
                    Urls = urls.Select(url => new UrlEntry { Protocol = url.Protocol.ToString(), Url = url.Url })
                        .ToList()
                };

                assetEntry.StreamingLocators.Add(StreamingLocatorInfo);
            }

            return assetEntry;
        }
    }
}