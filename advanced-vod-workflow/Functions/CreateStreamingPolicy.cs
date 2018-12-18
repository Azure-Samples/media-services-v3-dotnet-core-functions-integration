//
// Azure Media Services REST API v3 Functions
//
// CreateStreamingPolicy - This function creates an StreamingPolicy object.
//
/*
```c#
Input:
    {
        // [Required] The name of the streaming policy.
        "streamingPolicyName": "SharedStreamingForClearKey",
        
        // [Required] The mode for creating the transform.
        // Allowed values: "simple" or "advanced".
        // Default value: "simple".
        "mode": "simple",

        // Default Content Key used by current streaming policy.
        "defaultContentKeyPolicyName": "SharedContentKeyPolicyForClearKey",

        //
        // [mode = simple]
        //
        // Semi-colon-separated list of enabled protocols for NoEncryption.
        // Allowed values: Download, Dash, HLS, SmoothStreaming.
        "noEncryptionProtocols": "Dash;HLS;SmoothStreaming"

        //
        // [mode = simple]
        //
        //
        // Common Encryption CBCS Arguments
        //
        // The JSON representing which tracks should not be encrypted.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#trackselection.
        "cbcsClearTracks": {},
        // Label to specify Default Content Key for an encryption scheme.
        "cbcsDefaultKeyLabel": "cbcsKeyDefault",
        // Policy used by Default Content Key.
        "cbcsDefaultKeyPolicyName": null,
        // The JSON representing a list of StreamingPolicyContentKey.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#streamingpolicycontentkey.
        "cbcsKeyToTrackMappings": {},
        // Allows the license to be persistent or not.
        // Allowed values: false, true.
        "cbcsFairPlayAllowPersistentLicense": false,
        // The custom license acquisition URL template for a customer service to deliver keys to end users.
        // Not needed when using Azure Media Services for issuing keys.
        "cbcsFairPlayTemplate": null,
        // Custom attributes for PlayReady.
        "cbcsPlayReadyAttributes": null,
        // The custom license acquisition URL template for a customer service to deliver keys to end users.
        // Not needed when using Azure Media Services for issuing keys.
        "cbcsPlayReadyTemplate": null,
        // The custom license acquisition URL template for a customer service to deliver keys to end users.
        // Not needed when using Azure Media Services for issuing keys.
        "cbcsWidevineTemplate": null,
        // Semi-colon-separated list of enabled protocols for CommonEncryption CBCS.
        // Allowed values: Dash, HLS, SmoothStreaming.
        "cbcsProtocols": "Dash;HLS;SmoothStreaming",

        //
        // [mode = simple]
        //
        //
        // Common Encryption CENC Arguments
        //
        // The JSON representing which tracks should not be encrypted.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#trackselection.
        "cencClearTracks": {},
        // Label to specify Default Content Key for an encryption scheme.
        "cencDefaultKeyLabel": "cencKeyDefault",
        // Policy used by Default Content Key.
        "cencDefaultKeyPolicyName": null,
        // The JSON representing a list of StreamingPolicyContentKey.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#streamingpolicycontentkey.
        "cencKeyToTrackMappings": {},
        // Custom attributes for PlayReady.
        "cencPlayReadyAttributes": null,
        // The custom license acquisition URL template for a customer service to deliver keys to end users.
        // Not needed when using Azure Media Services for issuing keys.
        "cencPlayReadyTemplate": null,
        // The custom license acquisition URL template for a customer service to deliver keys to end users.
        // Not needed when using Azure Media Services for issuing keys.
        "cencWidevineTemplate": null,
        // Semi-colon-separated list of enabled protocols for CommonEncryption CENC.
        // Allowed values: Dash, HLS, SmoothStreaming.
        "cencProtocols": "Dash;HLS;SmoothStreaming",
        
        //
        // [mode = simple]
        //
        //
        // Envelope Encryption Arguments
        //
        // The JSON representing which tracks should not be encrypted.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#trackselection.
        "envelopeClearTracks": {},
        // Label to specify Default Content Key for an encryption scheme.
        "envelopeDefaultKeyLabel": "cencKeyDefault",
        // Policy used by Default Content Key.
        "envelopeDefaultKeyPolicyName": null,
        // The JSON representing a list of StreamingPolicyContentKey.
        // For further information about the JSON structure please refer to swagger documentation on
        // https://docs.microsoft.com/en-us/rest/api/media/streamingpolicies/create#streamingpolicycontentkey.
        "envelopeKeyToTrackMappings": {},
        // The KeyAcquistionUrlTemplate is used to point to user specified service to delivery content keys.
        "envelopeTemplate": null,
        // Semi-colon-separated list of enabled protocols for CommonEncryption CENC.
        // Allowed values: Dash, HLS, SmoothStreaming.
        "envelopeProtocols": "Dash;HLS;SmoothStreaming",

        //
        // [mode = advanced]
        //
        // Streaming Configuration option of Common Encryption CBCS
        "jsonCommonEncryptionCbcs": {
            "enabledProtocols": {
                "download": false,
                "dash": false,
                "hls": true,
                "smoothStreaming": false
            },
            "contentKeys": {
                "defaultKey": {
                    "label": "cbcsDefaultKey"
                }
            },
            "drm": {
                "fairPlay": {
                    "allowPersistentLicense": true
                }
            }
        }
        // Streaming Configuration option of Common Encryption CENC
        "jsonCommonEncryptionCenc": {
            "enabledProtocols": {
                "download": false,
                "dash": true,
                "hls": false,
                "smoothStreaming": true
            },
            "contentKeys": {
                "defaultKey": {
                    "label": "cencDefaultKey"
                }
            },
            "drm": {
                "playReady": {},
                "widevine": {}
            }
        }
        // Streaming Configuration option of Envelope Encryption
        "jsonEnvelopeEncryption": {}
        // Streaming Configuration option of No Encryption
        "jsonNoEncryption": {}
    }
Output:
    {
        // The name of the streaming policy.
        "streamingPolicyName": "SharedStreamingForClearKey",

        // The identifier of the streaming policy.
        "streamingPolicyId": "9d6a2b92-d61a-4e87-8348-7155c137f9ca",
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class CreateStreamingPolicy
    {
        [FunctionName("CreateStreamingPolicy")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - CreateStreamingPolicy was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.streamingPolicyName == null)
                return new BadRequestObjectResult("Please pass streamingPolicyName in the input object");
            if (data.defaultContentKeyPolicyName == null)
                return new BadRequestObjectResult("Please pass defaultContentKeyPolicyName in the input object");
            string streamingPolicyName = data.streamingPolicyName;
            string defaultContentKeyPolicyName = data.defaultContentKeyPolicyName;

            string mode = data.mode;
            if (mode != "simple" && mode != "advanced")
                return new BadRequestObjectResult("Please pass valid mode in the input object");
            //
            // Simple Mode
            //
            if (mode == "simple")
            {
                if (data.noEncryptionProtocols == null
                        && data.cbcsProtocols == null
                        && data.cencProtocols == null
                        && data.envelopeProtocols == null)
                    return new BadRequestObjectResult("Please pass one protocol for any encryption scheme at least in the input object");
            }
            //
            // Advanced Mode
            //
            if (mode == "advanced")
            {
                if (data.jsonNoEncryption == null
                    && data.jsonCommonEncryptionCbcs == null
                    && data.jsonCommonEncryptionCenc == null
                    && data.jsonEnvelopeEncryption == null)
                    return new BadRequestObjectResult("Please pass one encryption scheme JSON at least in the input object");
            }

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            StreamingPolicy policy = null;

            JsonConverter[] jsonReaders = {
                new MediaServicesHelperJsonReader(),
                new MediaServicesHelperTimeSpanJsonConverter()
            };

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                policy = client.StreamingPolicies.Get(amsconfig.ResourceGroup, amsconfig.AccountName, streamingPolicyName);
                if (policy == null)
                {
                    StreamingPolicy parameters = new StreamingPolicy();
                    parameters.DefaultContentKeyPolicyName = defaultContentKeyPolicyName;

                    if (mode == "simple")
                    {
                        // NoEncryption Arguments
                        if (data.noEncryptionProtocols != null)
                        {
                            String[] noEncryptionProtocols = data.noEncryptionProtocols.ToString().Split(';');
                            if (Array.IndexOf(noEncryptionProtocols, "Dash") > -1) parameters.NoEncryption.EnabledProtocols.Dash = true;
                            if (Array.IndexOf(noEncryptionProtocols, "Download") > -1) parameters.NoEncryption.EnabledProtocols.Download = true;
                            if (Array.IndexOf(noEncryptionProtocols, "Hls") > -1) parameters.NoEncryption.EnabledProtocols.Hls = true;
                            if (Array.IndexOf(noEncryptionProtocols, "SmoothStreaming") > -1) parameters.NoEncryption.EnabledProtocols.SmoothStreaming = true;
                        }

                        // Common Encryption CBCS Argument
                        if (data.cbcsClearTracks != null)
                        {
                            List<TrackSelection> tracks = JsonConvert.DeserializeObject<List<TrackSelection>>(data.cbcsClearTracks.ToString(), jsonReaders);
                            parameters.CommonEncryptionCbcs.ClearTracks = tracks;
                        }
                        if (data.cbcsDefaultKeyLabel != null) parameters.CommonEncryptionCbcs.ContentKeys.DefaultKey.Label = data.cbcsDefaultKeyLabel;
                        if (data.cbcsDefaultKeyPolicyName != null) parameters.CommonEncryptionCbcs.ContentKeys.DefaultKey.PolicyName = data.cbcsDefaultKeyPolicyName;
                        if (data.cbcsClearTracks != null)
                        {
                            List<StreamingPolicyContentKey> mappings = JsonConvert.DeserializeObject<List<StreamingPolicyContentKey>>(data.cbcsKeyToTrackMappings.ToString(), jsonReaders);
                            parameters.CommonEncryptionCbcs.ContentKeys.KeyToTrackMappings = mappings;
                        }
                        if (data.cbcsFairPlayTemplate != null)
                            parameters.CommonEncryptionCbcs.Drm.FairPlay.CustomLicenseAcquisitionUrlTemplate = data.cbcsFairPlayTemplate;
                        if (data.cbcsFairPlayAllowPersistentLicense != null)
                            parameters.CommonEncryptionCbcs.Drm.FairPlay.AllowPersistentLicense = data.cbcsFairPlayAllowPersistentLicense;
                        if (data.cbcsPlayReadyTemplate != null)
                            parameters.CommonEncryptionCbcs.Drm.PlayReady.CustomLicenseAcquisitionUrlTemplate = data.cbcsPlayReadyTemplate;
                        if (data.cbcsPlayReadyAttributes != null)
                            parameters.CommonEncryptionCbcs.Drm.PlayReady.PlayReadyCustomAttributes = data.cbcsPlayReadyAttributes;
                        if (data.cbcsWidevineTemplate != null)
                            parameters.CommonEncryptionCbcs.Drm.Widevine.CustomLicenseAcquisitionUrlTemplate = data.cbcsWidevineTemplate;
                        if (data.cbcsProtocols != null)
                        {
                            String[] commonEncryptionCbcsProtocols = data.cbcsProtocols.ToString().Split(';');
                            if (Array.IndexOf(commonEncryptionCbcsProtocols, "Dash") > -1) parameters.CommonEncryptionCbcs.EnabledProtocols.Dash = true;
                            if (Array.IndexOf(commonEncryptionCbcsProtocols, "Download") > -1) parameters.CommonEncryptionCbcs.EnabledProtocols.Download = true;
                            if (Array.IndexOf(commonEncryptionCbcsProtocols, "Hls") > -1) parameters.CommonEncryptionCbcs.EnabledProtocols.Hls = true;
                            if (Array.IndexOf(commonEncryptionCbcsProtocols, "SmoothStreaming") > -1) parameters.CommonEncryptionCbcs.EnabledProtocols.SmoothStreaming = true;
                        }

                        // Common Encryption CENC Argument
                        if (data.cencClearTracks != null)
                        {
                            List<TrackSelection> tracks = JsonConvert.DeserializeObject<List<TrackSelection>>(data.cencClearTracks.ToString(), jsonReaders);
                            parameters.CommonEncryptionCenc.ClearTracks = tracks;
                        }
                        if (data.cencDefaultKeyLabel != null) parameters.CommonEncryptionCenc.ContentKeys.DefaultKey.Label = data.cencDefaultKeyLabel;
                        if (data.cencDefaultKeyPolicyName != null) parameters.CommonEncryptionCenc.ContentKeys.DefaultKey.PolicyName = data.cencDefaultKeyPolicyName;
                        if (data.cencClearTracks != null)
                        {
                            List<StreamingPolicyContentKey> mappings = JsonConvert.DeserializeObject<List<StreamingPolicyContentKey>>(data.cencKeyToTrackMappings.ToString(), jsonReaders);
                            parameters.CommonEncryptionCenc.ContentKeys.KeyToTrackMappings = mappings;
                        }
                        if (data.cencPlayReadyTemplate != null)
                            parameters.CommonEncryptionCenc.Drm.PlayReady.CustomLicenseAcquisitionUrlTemplate = data.cencPlayReadyTemplate;
                        if (data.cencPlayReadyAttributes != null)
                            parameters.CommonEncryptionCenc.Drm.PlayReady.PlayReadyCustomAttributes = data.cencPlayReadyAttributes;
                        if (data.cencWidevineTemplate != null)
                            parameters.CommonEncryptionCenc.Drm.Widevine.CustomLicenseAcquisitionUrlTemplate = data.cencWidevineTemplate;
                        if (data.cencProtocols != null)
                        {
                            String[] commonEncryptionCencProtocols = data.cencProtocols.ToString().Split(';');
                            if (Array.IndexOf(commonEncryptionCencProtocols, "Dash") > -1) parameters.CommonEncryptionCenc.EnabledProtocols.Dash = true;
                            if (Array.IndexOf(commonEncryptionCencProtocols, "Download") > -1) parameters.CommonEncryptionCenc.EnabledProtocols.Download = true;
                            if (Array.IndexOf(commonEncryptionCencProtocols, "Hls") > -1) parameters.CommonEncryptionCenc.EnabledProtocols.Hls = true;
                            if (Array.IndexOf(commonEncryptionCencProtocols, "SmoothStreaming") > -1) parameters.CommonEncryptionCenc.EnabledProtocols.SmoothStreaming = true;
                        }

                        // Envelope Encryption Argument
                        if (data.envelopeClearTracks != null)
                        {
                            List<TrackSelection> tracks = JsonConvert.DeserializeObject<List<TrackSelection>>(data.envelopeClearTracks.ToString(), jsonReaders);
                            parameters.EnvelopeEncryption.ClearTracks = tracks;
                        }
                        if (data.envelopeDefaultKeyLabel != null) parameters.EnvelopeEncryption.ContentKeys.DefaultKey.Label = data.envelopeDefaultKeyLabel;
                        if (data.envelopeDefaultKeyPolicyName != null) parameters.EnvelopeEncryption.ContentKeys.DefaultKey.PolicyName = data.envelopeDefaultKeyPolicyName;
                        if (data.envelopeClearTracks != null)
                        {
                            List<StreamingPolicyContentKey> mappings = JsonConvert.DeserializeObject<List<StreamingPolicyContentKey>>(data.envelopeKeyToTrackMappings.ToString(), jsonReaders);
                            parameters.EnvelopeEncryption.ContentKeys.KeyToTrackMappings = mappings;
                        }
                        if (data.envelopeTemplate != null)
                            parameters.EnvelopeEncryption.CustomKeyAcquisitionUrlTemplate = data.envelopeTemplate;
                        if (data.envelopeProtocols != null)
                        {
                            String[] envelopeEncryptionProtocols = data.envelopeProtocols.ToString().Split(';');
                            if (Array.IndexOf(envelopeEncryptionProtocols, "Dash") > -1) parameters.EnvelopeEncryption.EnabledProtocols.Dash = true;
                            if (Array.IndexOf(envelopeEncryptionProtocols, "Download") > -1) parameters.EnvelopeEncryption.EnabledProtocols.Download = true;
                            if (Array.IndexOf(envelopeEncryptionProtocols, "Hls") > -1) parameters.EnvelopeEncryption.EnabledProtocols.Hls = true;
                            if (Array.IndexOf(envelopeEncryptionProtocols, "SmoothStreaming") > -1) parameters.EnvelopeEncryption.EnabledProtocols.SmoothStreaming = true;
                        }

                    }
                    else if (mode == "advanced")
                    {
                        NoEncryption noEncryptionArguments = null;
                        CommonEncryptionCbcs commonEncryptionCbcsArguments = null;
                        CommonEncryptionCenc commonEncryptionCencArguments = null;
                        EnvelopeEncryption envelopeEncryptionArguments = null;

                        if (data.jsonNoEncryption != null) noEncryptionArguments = JsonConvert.DeserializeObject<NoEncryption>(data.configNoEncryption.ToString(), jsonReaders);
                        if (data.jsonCommonEncryptionCbcs != null) commonEncryptionCbcsArguments = JsonConvert.DeserializeObject<CommonEncryptionCbcs>(data.jsonCommonEncryptionCbcs.ToString(), jsonReaders);
                        if (data.jsonCommonEncryptionCenc != null) commonEncryptionCencArguments = JsonConvert.DeserializeObject<CommonEncryptionCenc>(data.configCommonEncryptionCenc.ToString(), jsonReaders);
                        if (data.jsonEnvelopeEncryption != null) envelopeEncryptionArguments = JsonConvert.DeserializeObject<EnvelopeEncryption>(data.configEnvelopeEncryption.ToString(), jsonReaders);
                        parameters.NoEncryption = noEncryptionArguments;
                        parameters.CommonEncryptionCbcs = commonEncryptionCbcsArguments;
                        parameters.CommonEncryptionCenc = commonEncryptionCencArguments;
                        parameters.EnvelopeEncryption = envelopeEncryptionArguments;
                    }
                    parameters.Validate();
                    policy = client.StreamingPolicies.Create(amsconfig.ResourceGroup, amsconfig.AccountName, streamingPolicyName, parameters);
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

            return (ActionResult)new OkObjectResult(new
            {
                streamingPolicyName = streamingPolicyName,
                streamingPolicyId = policy.Id
            });
        }
    }
}