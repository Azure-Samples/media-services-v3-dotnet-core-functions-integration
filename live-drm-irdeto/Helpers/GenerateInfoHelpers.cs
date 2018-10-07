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

namespace LiveDrmOperationsV3
{
    class GenerateInfoHelpers
    {
        public static LiveDRMIrdeto.Models.GeneralOutputInfo GenerateOutputInformation(ConfigWrapper config, IAzureMediaServicesClient client, List<LiveEvent> liveEvents)
        {
            var generalOutputInfo = new LiveDRMIrdeto.Models.GeneralOutputInfo();
            generalOutputInfo.Success = true;
            generalOutputInfo.LiveEvents = new List<LiveDRMIrdeto.Models.LiveEventEntry>();

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
                var liveEventInfo = new LiveDRMIrdeto.Models.LiveEventEntry()
                {
                    Name = liveEvent.Name,
                    ResourceState = liveEvent.ResourceState.ToString(),
                    vanityUrl = liveEvent.VanityUrl,
                    Input = liveEvent.Input.Endpoints.Select(endPoint => new LiveDRMIrdeto.Models.Input() { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    InputACL = liveEvent.Input.AccessControl?.Ip.Allow.Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    Preview = previewE.Select(endPoint => new LiveDRMIrdeto.Models.Preview() { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    PreviewACL = liveEvent.Preview.AccessControl?.Ip.Allow.Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    LiveOutputs = new List<LiveDRMIrdeto.Models.LiveOutputEntry>(),
                    AMSAccountName = config.AccountName,
                    Region = config.Region,
                    ResourceGroup = config.ResourceGroup
                };
                generalOutputInfo.LiveEvents.Add(liveEventInfo);

                foreach (var liveOutput in liveOutputs)
                {
                    List<InputUrl> urls = new List<InputUrl>();
                    string streamingLocatorName = null;
                    string streamingPolicyName = null;
                    string cenckeyId = "";
                    string cbcskeyId = "";
                    StreamingPolicy streamingPolicy = null;

                    //var locators = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, liveOutput.AssetName);

                    streamingLocatorName = IrdetoHelpers.ReturnLocatorNameFromDescription(liveOutput);
                    if (streamingLocatorName != null)
                    {
                        var streamingLocator = client.StreamingLocators.Get(config.ResourceGroup, config.AccountName, streamingLocatorName);
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

                    var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, liveOutput.AssetName); // to get info on storage

                    // output info
                    var liveOutputInfo = new LiveDRMIrdeto.Models.LiveOutputEntry()
                    {
                        Name = liveOutput.Name,
                        ResourceState = liveOutput.ResourceState,
                        ArchiveWindowLength = (int)liveOutput.ArchiveWindowLength.TotalMinutes,
                        AssetName = liveOutput.AssetName,
                        AssetStorageAccountName = asset?.StorageAccountName,
                        StreamingLocatorName = streamingLocatorName,
                        StreamingPolicyName = streamingPolicyName,
                        Drm = new List<LiveDRMIrdeto.Models.Drm>()
                            {
                                new LiveDRMIrdeto.Models.Drm(){ Type="FairPlay", LicenseUrl = streamingPolicy?.CommonEncryptionCbcs.Drm.FairPlay.CustomLicenseAcquisitionUrlTemplate.Replace("{ContentKeyId}",cbcskeyId) },
                                new LiveDRMIrdeto.Models.Drm(){ Type="PlayReady", LicenseUrl = streamingPolicy?.CommonEncryptionCenc.Drm.PlayReady.CustomLicenseAcquisitionUrlTemplate },
                                new LiveDRMIrdeto.Models.Drm(){ Type="Widevine", LicenseUrl = streamingPolicy?.CommonEncryptionCenc.Drm.Widevine.CustomLicenseAcquisitionUrlTemplate }
                            },
                        CencKeyId = cenckeyId,
                        CbcsKeyId = cbcskeyId,
                        Urls = urls.Select(url => new LiveDRMIrdeto.Models.UrlEntry() { Protocol = url.Protocol, Url = url.Url }).ToList()
                    };
                    liveEventInfo.LiveOutputs.Add(liveOutputInfo);
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
