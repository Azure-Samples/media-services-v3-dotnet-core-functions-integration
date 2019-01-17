//
// Azure Media Services REST API v3 Functions
//
// CreateWidevineLicenseTemplate - This function creates a Widevine License Template JSON data.
//
// Reference: https://storage.googleapis.com/wvdocs/Widevine_DRM_Proxy_Integration.pdf
//
/*
```c#
Input:
    {
        // Controls which content keys should be included in a license.
        // Allowed values:
        //      SD_ONLY - returns SD and AUDIO keys only
        //      SD_HD - returns SD, HD and AUDIO keys only
        //      SD_UHD1 - returns SD, HD, UHD1 and AUDIO keys
        //      SD_UHD2 - returns SD, HD, UHD1, UHD2 and AUDIO keys (all)
        "allowedTrackTypes": "SD_HD",

        // A finer grained control on what content keys to return.
        // Semi-colon-separated triplet string presentation (TrackType:SecurityLevel:HDCP) is required.
        // See Content Key Spec in the reference for details.
        "contentKeySpecs": "SD:1:HDCP_NONE;HD:1:HDCP_V2",

        // Policy Overrides
        // Policies settings for this license.
        // In the event this asset has a predefined policy, these specified values will be used.

        // Indicates that playback of the content is allowed.
        // Allowed values: Boolean - true or false
        // Default value: false.
        "canPlay": false,

        // Indicates that the license may be persisted to non-volatile storage for offline use.
        // Allowed values: Boolean - true or false
        // Default value: false.
        "canPersist": false,

        // Indicates that renewal of this license is allowed.
        // If true, the duration of the license can be extended by heartbeat.
        // Allowed values: Boolean - true or false
        // Default value: false.
        "canRenew": false,

        // Indicates the time window for this specific license.
        // A value of 0 indicates unlimited.
        // Default value: 0.
        "licenseDurationSeconds": 0,

        // Indicates the time window while playback is permitted.
        // A value of 0 indicates unlimited.
        // Default value: 0.
        "rentalDurationSeconds": 0,

        // The viewing window of time once playback starts within the license duration.
        // A value of 0 indicates unlimited.
        // Default value: 0.
        "playbackDurationSeconds": 0,

        // All heartbeat (renewal) requests for this license shall be directed to the specified URL.
        // This field is only used if canRenew is true.
        "renewalServerUrl": null,

        // How many seconds after license_start_time, before renewal is first attempted.
        // This field is only used if canRenew is true.
        // Default value: 0.
        "renewalDelaySeconds": 0,

        // Specifies the delay in seconds between subsequent license renewal requests, in case of failure.
        // This field is only used if canRenew is true.
        // Default value: 0.
        "renewalRetryIntervalSeconds": 0,

        // The window of time, in which playback is allowed to continue while renewal is attempted,
        // yet unsuccessful due to backend problems with the license server.
        // A value of 0 indicates unlimited.
        // This field is only used if canRenew is true.
        // Default value: 0.
        "renewalRecoveryDurationSeconds": 0,
        
        // Indicates that the license shall be sent for renewal when usage is started.
        // This field is only used if canRenew is true.
        // Allowed values: Boolean - true or false
        // Default value: false.
        "renewWithUsage": false,
        
        // Indicates to client that license renewal and release requests must include client identification (client_id).
        // Allowed values: Boolean - true or false
        // Default value: false.
        "alwaysIncludeClientId": false
    }
Output:
    {
        // The JSON string of Widevine License Template.
        "widevineLicenses": { ... }

    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;
using advanced_vod_functions_v3.SharedLibs.Widevine;


namespace advanced_vod_functions_v3.Functions
{
    public static class CreateWidevineLicenseTemplate
    {
        [FunctionName("CreateWidevineLicenseTemplate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AMS v3 Function - CreateWidevineLicenseTemplate was triggered!");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            List<ContentKeyPolicyPlayReadyLicense> licenseTemplates = new List<ContentKeyPolicyPlayReadyLicense>();

            JToken jToken = null;
            JsonConverter[] jsonReaders = {
                new MediaServicesHelperJsonReader(),
                new MediaServicesHelperTimeSpanJsonConverter()
            };
            JsonConverter[] jsonWriter = {
                new MediaServicesHelperJsonWriter(),
                new MediaServicesHelperTimeSpanJsonConverter()
            };

            try
            {
                WidevineLicenseTemplate widevineLicenseTemplate = new WidevineLicenseTemplate();
                widevineLicenseTemplate.PolicyOverrides = new PolicyOverrides();

                if (data.allowedTrackTypes != null) widevineLicenseTemplate.AllowedTrackTypes = data.allowedTrackTypes;
                if (data.contentKeySpecs != null)
                {
                    List<ContentKeySpec> spec = new List<ContentKeySpec>();
                    string[] contentKeySpecs = data.contentKeySpecs.ToString().Split(';');
                    foreach (string contentKeySpec in contentKeySpecs)
                    {
                        spec.Add(ContentKeySpec.ToContentKeySpec(contentKeySpec));
                    }
                    widevineLicenseTemplate.ContentKeySpecs = spec.ToArray();
                }
                if (data.canPlay != null) widevineLicenseTemplate.PolicyOverrides.CanPlay = data.canPlay;
                if (data.canPersist != null) widevineLicenseTemplate.PolicyOverrides.CanPersist = data.canPersist;
                if (data.canRenew != null) widevineLicenseTemplate.PolicyOverrides.CanRenew = data.canRenew;
                if (data.licenseDurationSeconds != null) widevineLicenseTemplate.PolicyOverrides.LicenseDurationSeconds = data.licenseDurationSeconds;
                if (data.rentalDurationSeconds != null) widevineLicenseTemplate.PolicyOverrides.RentalDurationSeconds = data.rentalDurationSeconds;
                if (data.playbackDurationSeconds != null) widevineLicenseTemplate.PolicyOverrides.PlaybackDurationSeconds = data.playbackDurationSeconds;
                if (data.renewalServerUrl != null) widevineLicenseTemplate.PolicyOverrides.RenewalServerUrl = data.renewalServerUrl;
                if (data.renewalDelaySeconds != null) widevineLicenseTemplate.PolicyOverrides.RenewalDelaySeconds = data.renewalDelaySeconds;
                if (data.renewalRetryIntervalSeconds != null) widevineLicenseTemplate.PolicyOverrides.RenewalRetryIntervalSeconds = data.renewalRetryIntervalSeconds;
                if (data.renewalRecoveryDurationSeconds != null) widevineLicenseTemplate.PolicyOverrides.RenewalRecoveryDurationSeconds = data.renewalRecoveryDurationSeconds;
                if (data.renewWithUsage != null) widevineLicenseTemplate.PolicyOverrides.RenewWithUsage = data.renewWithUsage;
                if (data.alwaysIncludeClientId != null) widevineLicenseTemplate.PolicyOverrides.AlwaysIncludeClientId = data.alwaysIncludeClientId;

                widevineLicenseTemplate.Validate();

                string jsonString = JsonConvert.SerializeObject(widevineLicenseTemplate);
                jToken = JToken.Parse(jsonString);
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

            return (ActionResult)new OkObjectResult(new
            {
                widevineLicenses = jToken
            });
        }
    }
}
