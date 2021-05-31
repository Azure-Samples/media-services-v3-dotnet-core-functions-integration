using System.Collections.Generic;
using System.Linq;
using LiveDrmOperationsV3.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;

namespace LiveDrmOperationsV3.Helpers
{
    internal partial class GenerateInfoHelpers
    {
        public static GeneralOutputInfo GenerateOutputInformation(ConfigWrapper config,
            IAzureMediaServicesClient client, List<LiveEvent> liveEvents)
        {
            var generalOutputInfo = new GeneralOutputInfo { Success = true, LiveEvents = new List<LiveEventEntry>() };

            foreach (var liveEventenum in liveEvents)
            {
                var liveEvent =
                    client.LiveEvents.Get(config.ResourceGroup, config.AccountName,
                        liveEventenum.Name); // we refresh the object

                var liveOutputs = client.LiveOutputs.List(config.ResourceGroup, config.AccountName, liveEvent.Name);

                // let's provide DASH format for preview
                var previewE = liveEvent.Preview.Endpoints
                    .Select(a => new PreviewInfo { Protocol = a.Protocol, Url = a.Url }).ToList();
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
                    LiveEventName = liveEvent.Name,
                    
                    UseStaticHostname = liveEvent.UseStaticHostname,
                    Input = liveEvent.Input.Endpoints
                        .Select(endPoint => new UrlEntry { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    InputACL = liveEvent.Input.AccessControl?.Ip.Allow
                        .Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    Preview = previewE
                        .Select(endPoint => new UrlEntry { Protocol = endPoint.Protocol, Url = endPoint.Url }).ToList(),
                    PreviewACL = liveEvent.Preview.AccessControl?.Ip.Allow
                        .Select(ip => ip.Address + "/" + ip.SubnetPrefixLength).ToList(),
                    LiveOutputs = new List<LiveOutputEntry>(),
                    AMSAccountName = config.AccountName,
                    Region = config.Region,
                    ResourceGroup = config.ResourceGroup,
                    LowLatency = liveEvent.StreamOptions?.Contains(StreamOptionsFlag.LowLatency)
                };
                generalOutputInfo.LiveEvents.Add(liveEventInfo);

                foreach (var liveOutput in liveOutputs)
                {
                    var urls = new List<OutputUrl>();
                    string streamingPolicyName = null;
                    StreamingPolicy streamingPolicy = null;

                    var asset = client.Assets.Get(config.ResourceGroup, config.AccountName, liveOutput.AssetName);

                    // output info
                    var liveOutputInfo = new LiveOutputEntry
                    {
                        LiveOutputName = liveOutput.Name,
                        ResourceState = liveOutput.ResourceState,
                        ArchiveWindowLength = (int)liveOutput.ArchiveWindowLength.TotalMinutes,
                        AssetName = liveOutput.AssetName,
                        AssetStorageAccountName = asset?.StorageAccountName,
                        StreamingLocators = new List<StreamingLocatorEntry>()
                    };
                    liveEventInfo.LiveOutputs.Add(liveOutputInfo);

                    var streamingLocatorsNames = client.Assets.ListStreamingLocators(config.ResourceGroup, config.AccountName, liveOutput.AssetName).StreamingLocators.Select(l => l.Name);
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

                                urls = MediaServicesHelpers.GetUrls(config, client, streamingLocator,
                                    liveOutput.ManifestName, true, true, true, true, true);
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