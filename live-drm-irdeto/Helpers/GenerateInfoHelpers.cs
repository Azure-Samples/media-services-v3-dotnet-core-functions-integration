using Microsoft.Azure.Management.Media;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using LiveDrmOperationsV3.Models;


namespace LiveDrmOperationsV3.Helpers
{
    class GenerateInfoHelpers
    {
        public static GeneralOutputInfo GenerateOutputInformation(ConfigWrapper config, IAzureMediaServicesClient client, List<LiveEvent> liveEvents)
        {
            var generalOutputInfo = new GeneralOutputInfo();
            generalOutputInfo.Success = true;
            generalOutputInfo.LiveEvents = new List<LiveEventEntry>();

            foreach (var liveEventenum in liveEvents)
            {
                var liveEvent = client.LiveEvents.Get(config.ResourceGroup, config.AccountName, liveEventenum.Name); // we refresh the object

                var liveOutputs = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEvent.Name);

                // let's provide DASH format for preview
                var previewE = liveEvent.Preview.Endpoints.Select(a => new PreviewInfo() { Protocol = a.Protocol, Url = a.Url }).ToList();
                /*
                // Code to expose Preview DASH and not Smooth
                if (previewE.Count == 1 && previewE[0].Protocol == "FragmentedMP4")
                {
                    previewE[0].Protocol = "DashCsf";
                    previewE[0].Url = previewE[0].Url + "(format=mpd-time-csf)";
                }
                */


                // output info
                var liveEventInfo = new LiveEventEntry()
                {
                    Name = liveEvent.Name,
                    ResourceState = liveEvent.ResourceState.ToString(),
                    VanityUrl = liveEvent.VanityUrl,
                    Input = liveEvent.Input.Endpoints.Select(endPoint => new Input() { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    InputACL = liveEvent.Input.AccessControl?.Ip.Allow.Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    Preview = previewE.Select(endPoint => new Preview() { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    PreviewACL = liveEvent.Preview.AccessControl?.Ip.Allow.Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    LiveOutputs = new List<LiveOutputEntry>(),
                    AMSAccountName = config.AccountName,
                    Region = config.Region,
                    ResourceGroup = config.ResourceGroup
                };
                generalOutputInfo.LiveEvents.Add(liveEventInfo);

                foreach (var liveOutput in liveOutputs)
                {
                    List<InputUrl> urls = new List<InputUrl>();
                    string streamingPolicyName = null;
                    string cenckeyId = null;
                    string cbcskeyId = null;
                    StreamingPolicy streamingPolicy = null;

                    var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, liveOutput.AssetName);

                    // output info
                    var liveOutputInfo = new LiveOutputEntry()
                    {
                        Name = liveOutput.Name,
                        ResourceState = liveOutput.ResourceState,
                        ArchiveWindowLength = (int)liveOutput.ArchiveWindowLength.TotalMinutes,
                        AssetName = liveOutput.AssetName,
                        AssetStorageAccountName = asset?.StorageAccountName,
                        StreamingLocators = new List<StreamingLocatorEntry>(),
                        CencKeyId = cenckeyId,
                        CbcsKeyId = cbcskeyId,
                    };
                    liveEventInfo.LiveOutputs.Add(liveOutputInfo);

                    //var locators = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, liveOutput.AssetName);
                    //streamingLocatorName = locators.StreamingLocators.FirstOrDefault().Name;
                    var streamingLocatorsNames = IrdetoHelpers.ReturnLocatorNameFromDescription(asset);
                    foreach (var locatorName in streamingLocatorsNames)
                    {
                        if (locatorName != null)
                        {
                            var streamingLocator = client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, locatorName);
                            if (streamingLocator != null)
                            {
                                streamingPolicyName = streamingLocator.StreamingPolicyName;

                                if (streamingLocator.ContentKeys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCenc).FirstOrDefault() != null && streamingLocator.ContentKeys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCbcs).FirstOrDefault() != null)
                                {
                                    cenckeyId = streamingLocator.ContentKeys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCenc).FirstOrDefault().Id.ToString();
                                    cbcskeyId = streamingLocator.ContentKeys.Where(k => k.LabelReferenceInStreamingPolicy == IrdetoHelpers.labelCbcs).FirstOrDefault().Id.ToString();
                                }
                                urls = IrdetoHelpers.GetUrls(config, client, streamingLocator, liveOutput.ManifestName, true, true, true, true, true);
                            }
                        }

                        if (streamingPolicyName != null)
                        {
                            streamingPolicy = client.StreamingPolicies.Get(config.ResourceGroup, config.AccountName, streamingPolicyName);
                        }

                        var drmlist = new List<Drm>();
                        if (streamingPolicy!=null)
                        {
                            if (streamingPolicy.CommonEncryptionCbcs != null)
                            {
                                drmlist.Add(new Drm() { Type = "FairPlay", LicenseUrl = streamingPolicy?.CommonEncryptionCbcs?.Drm.FairPlay.CustomLicenseAcquisitionUrlTemplate.Replace("{ContentKeyId}", cbcskeyId) });
                            }
                            if (streamingPolicy.CommonEncryptionCenc != null)
                            {
                                drmlist.Add(new Drm() { Type = "PlayReady", LicenseUrl = streamingPolicy?.CommonEncryptionCenc?.Drm.PlayReady.CustomLicenseAcquisitionUrlTemplate });
                                drmlist.Add(new Drm() { Type = "Widevine", LicenseUrl = streamingPolicy?.CommonEncryptionCenc?.Drm.Widevine.CustomLicenseAcquisitionUrlTemplate });
                            }
                        }

                        var StreamingLocatorInfo = new StreamingLocatorEntry()
                        {
                            Name = locatorName,
                            StreamingPolicyName = streamingPolicyName,
                            Drm = drmlist,
                            Urls = urls.Select(url => new UrlEntry() { Protocol = url.Protocol, Url = url.Url }).ToList()
                        };

                        liveOutputInfo.StreamingLocators.Add(StreamingLocatorInfo);
                    }
                }
            }
            return generalOutputInfo;
        }

        public class PreviewInfo
        {
            public string Protocol { get; set; }
            public string Url { get; set; }
        }
    }
}
