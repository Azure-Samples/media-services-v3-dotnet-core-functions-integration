//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using Microsoft.Rest;
using System;


namespace advanced_vod_functions_v3.SharedLibs.Widevine
{
    // https://storage.googleapis.com/wvdocs/Widevine_DRM_Proxy_Integration.pdf
    public class WidevineLicenseTemplate
    {
        // Controls which content keys should be included in a license.
        //  SD_ONLY - returns SD and AUDIO keys only
        //  SD_HD - returns SD, HD and AUDIO keys only
        //  SD_UHD1 - returns SD, HD, UHD1 and AUDIO keys
        //  SD_UHD2 - returns SD, HD, UHD1, UHD2 and AUDIO keys(all)
        [Newtonsoft.Json.JsonProperty("allowed_track_types", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string AllowedTrackTypes { get; set; }

        // A finer grained control on what content keys to return.
        // See Content Key Spec in thereference for details.
        // Only one of allowed_track_types and content_key_specs can be specified.
        [Newtonsoft.Json.JsonProperty("content_key_specs", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ContentKeySpec[] ContentKeySpecs { get; set; }

        // Policies settings for this license.
        // In the event this asset has a predefined policy, these specified values will be used.
        [Newtonsoft.Json.JsonProperty("policy_overrides", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public PolicyOverrides PolicyOverrides { get; set; }

        // Validate members.
        public virtual void Validate()
        {
            if (AllowedTrackTypes != null)
            {
                switch (AllowedTrackTypes)
                {
                    case "SD_ONLY":
                    case "SD_HD":
                    case "SD_UHD1":
                    case "SD_UHD2":
                        break;
                    default:
                        throw new ValidationException("WidevineLicenseTemplate: Invalid AllowedTrackTypes.");
                }
            }

            if (ContentKeySpecs != null)
                foreach (var s in ContentKeySpecs) s.Validate();
            if (PolicyOverrides != null) PolicyOverrides.Validate();
        }

    }
    public class PolicyOverrides
    {
        // Indicates that playback of the content is allowed.
        // Default = false.
        [Newtonsoft.Json.JsonProperty("can_play")]
        public bool CanPlay { get; set; }

        // Indicates that the license may be persisted to non-volatile storage for offline use.
        // Default = false.
        [Newtonsoft.Json.JsonProperty("can_persist", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public bool CanPersist { get; set; }

        // Indicates that renewal of this license is allowed.
        // If true, the duration of the license can be extended by heartbeat.
        // Default = false.
        [Newtonsoft.Json.JsonProperty("can_renew", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public bool CanRenew { get; set; }

        // Indicates the time window for this specific license.
        // A value of 0 indicates that there is no limit to the duration.
        // Default is 0.
        [Newtonsoft.Json.JsonProperty("license_duration_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int LicenseDurationSeconds { get; set; }

        // Indicates the time window while playback is permitted.
        // A value of 0 indicates that there is no limit to the duration.
        // Default is 0.
        [Newtonsoft.Json.JsonProperty("rental_duration_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RentalDurationSeconds { get; set; }

        // The viewing window of time after playback starts within the license duration.
        // A value of 0 indicates that there is no limit to the duration.
        // Default is 0.
        [Newtonsoft.Json.JsonProperty("playback_duration_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int PlaybackDurationSeconds { get; set; }

        // All heartbeat (renewal) requests for this license shall be directed to the specified URL.
        // This field is only used if can_renew is true.
        [Newtonsoft.Json.JsonProperty("renewal_server_url", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string RenewalServerUrl { get; set; }

        // How many seconds after license_start_time, before renewal is first attempted.
        // This field is only used if can_renew is true.
        // Default = 0.
        [Newtonsoft.Json.JsonProperty("renewal_delay_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RenewalDelaySeconds { get; set; }

        // Specifies the delay in seconds between subsequent license renewal requests, in case of failure.
        // This field is only used if can_renew is true.
        // Default = 0.
        [Newtonsoft.Json.JsonProperty("renewal_retry_interval_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RenewalRetryIntervalSeconds { get; set; }

        // The window of time, in which playback is allowed to continue while renewal is attempted,
        // yet unsuccessful due to backend problems with the license server.
        // A value of 0 indicates unlimited.
        // This field is only used if can_renew is true.
        // Default = 0.
        [Newtonsoft.Json.JsonProperty("renewal_recovery_duration_seconds", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int RenewalRecoveryDurationSeconds { get; set; }

        // Indicates that the license shall be sent for renewal when usage is started.
        // This field is only used if can_renew is true.
        // Default = false.
        [Newtonsoft.Json.JsonProperty("renew_with_usage", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public bool RenewWithUsage { get; set; }

        // Indicates to client that license renewal and release requests must include client identification(client_id).
        // Default = false.
        [Newtonsoft.Json.JsonProperty("always_include_client_id", DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public bool AlwaysIncludeClientId { get; set; }

        public virtual void Validate()
        {
            //
        }

    }

    public class ContentKeySpec
    {
        // A track type definition.
        // If content_key_specs is declared in the license request, you must specify all track types explicitly for playback.
        //  AUDIO - audio tracks
        //  SD - 576p or less
        //  HD - 720p, 1080p
        //  UHD1 - 4K
        //  UHD2 - 8K
        [Newtonsoft.Json.JsonProperty("track_type")]
        public string TrackType { get; set; }

        // Defines client robustness requirements for playback.
        //  1 - Software-based whitebox crypto is required. (SW_SECURE_CRYPTO)
        //  2 - Software crypto and an obfuscated decoder is required. (SW_SECURE_DECODE)
        //  3 - The key material and crypto operations must be performed within a hardware backed trusted execution environment. (HW_SECURE_CRYPTO)
        //  4 - The crypto and decoding of content must be performed within a hardware backed trusted execution environment. (HW_SECURE_DECODE)
        //  5 - The crypto, decoding and all handling of the media(compressed and uncompressed) must be handled within a hardware backed trusted execution environment. (HW_SECURE_ALL)
        // Default = 1.
        [Newtonsoft.Json.JsonProperty("security_level")]
        public int SecurityLevel { get; set; }

        // Required output protection
        [Newtonsoft.Json.JsonProperty("required_output_protection")]
        public OutputProtection RequiredOutputProtection { get; set; }

        public virtual void Validate()
        {
            if (TrackType == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "TrackType");
            }

            if (TrackType != null)
            {
                switch (TrackType)
                {
                    case "AUDIO":
                    case "SD":
                    case "HD":
                    case "UHD1":
                    case "UHD2":
                        break;
                    default:
                        throw new ValidationException("WidevineLicenseTemplate: Invalid TrackType.");
                }
            }

            if (SecurityLevel <= 0 || SecurityLevel > 5)
            {
                throw new ValidationException("WidevineLicenseTemplate: Invalid SecurityLevel.");
            }

            if (RequiredOutputProtection != null)
                RequiredOutputProtection.Validate();
        }

        public static ContentKeySpec ToContentKeySpec(string specString)
        {
            ContentKeySpec spec = new ContentKeySpec();
            string[] vals = specString.Split(':');
            if (vals.Length != 3) throw new ValidationException("WidevineLicenseTemplate: Invalid ContentKeySpec string input.");
            spec.TrackType = vals[0];
            spec.SecurityLevel = Convert.ToInt32(vals[1]);
            spec.RequiredOutputProtection = new OutputProtection();
            spec.RequiredOutputProtection.HDCP = vals[2];

            return spec;
        }
    }

    public class OutputProtection
    {
        // Indicates whether HDCP is required.
        // Allowed values:
        //      HDCP_NONE,
        //      HDCP_V1,
        //      HDCP_V2,
        //      HDCP_V2_1,
        //      HDCP_V2_2,
        //      HDCP_NO_DIGITAL_OUTPUT
        // Default = HDCP_NONE.
        // Note: These values must be capitalized.
        [Newtonsoft.Json.JsonProperty("hdcp")]
        public string HDCP { get; set; }

        public virtual void Validate()
        {
            if (HDCP == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "HDCP");
            }

            if (HDCP != null)
            {
                switch (HDCP)
                {
                    case "HDCP_NONE":
                    case "HDCP_V1":
                    case "HDCP_V2":
                    case "HDCP_V2_1":
                    case "HDCP_V2_2":
                    case "HDCP_NO_DIGITAL_OUTPUT":
                        break;
                    default:
                        throw new ValidationException("WidevineLicenseTemplate: Invalid HDCP.");
                }
            }
        }

    }
}
