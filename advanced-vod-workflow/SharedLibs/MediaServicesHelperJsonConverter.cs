//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using Microsoft.Azure.Management.Media.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace advanced_vod_functions_v3.SharedLibs
{
    public class JsonConvertionRule
    {
        public Type t;
        public Dictionary<string, Type> convertRules;
    }

    public class MediaServicesHelperJsonConverter : Newtonsoft.Json.JsonConverter
    {
        static List<JsonConvertionRule> conversions = new List<JsonConvertionRule>
        {
            // Codec
            new JsonConvertionRule
            {
                t = typeof(Codec),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.AacAudio", typeof(AacAudio) },
                    { "#Microsoft.Media.CopyAudio", typeof(CopyAudio) },
                    { "#Microsoft.Media.CopyVideo", typeof(CopyVideo) },
                    { "#Microsoft.Media.H264Video", typeof(H264Video) },
                    { "#Microsoft.Media.JpgImage", typeof(JpgImage) },
                    { "#Microsoft.Media.PngImage", typeof(PngImage) }
                }
            },
            // ContentKeyPolicyConfiguration
            new JsonConvertionRule
            {
                t = typeof(ContentKeyPolicyConfiguration),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyClearKeyConfiguration", typeof(ContentKeyPolicyClearKeyConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyFairPlayConfiguration", typeof(ContentKeyPolicyFairPlayConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyConfiguration", typeof(ContentKeyPolicyPlayReadyConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyWidevineConfiguration", typeof(ContentKeyPolicyWidevineConfiguration) }
                }
            },
            // ContentKeyPolicyRestriction
            new JsonConvertionRule
            {
                t = typeof(ContentKeyPolicyRestriction),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyOpenRestriction", typeof(ContentKeyPolicyOpenRestriction) },
                    { "#Microsoft.Media.ContentKeyPolicyTokenRestriction", typeof(ContentKeyPolicyTokenRestriction) }
                }
            },
            // ContentKeyPolicyRestrictionTokenKey
            new JsonConvertionRule
            {
                t = typeof(ContentKeyPolicyRestrictionTokenKey),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyRsaTokenKey", typeof(ContentKeyPolicyRsaTokenKey) },
                    { "#Microsoft.Media.ContentKeyPolicySymmetricTokenKey", typeof(ContentKeyPolicySymmetricTokenKey) },
                    { "#Microsoft.Media.ContentKeyPolicyX509CertificateTokenKey", typeof(ContentKeyPolicyX509CertificateTokenKey) }
                }
            },
            new JsonConvertionRule
            {
                t = typeof(ContentKeyPolicyPlayReadyContentKeyLocation),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader", typeof(ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader) },
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyContentEncryptionKeyFromKeyIdentifier", typeof(ContentKeyPolicyPlayReadyContentEncryptionKeyFromKeyIdentifier) }
                }
            },
            // Format
            new JsonConvertionRule
            {
                t = typeof(Format),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.Mp4Format", typeof(Mp4Format) },
                    { "#Microsoft.Media.TransportStreamFormat", typeof(TransportStreamFormat) },
                    { "#Microsoft.Media.JpgFormat", typeof(JpgFormat) },
                    { "#Microsoft.Media.PngFormat", typeof(PngFormat) }
                }
            },
            // JobInput
            new JsonConvertionRule
            {
                t = typeof(JobInput),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.JobInputAsset ", typeof(JobInputAsset) },
                    { "#Microsoft.Media.JobInputHttp", typeof(JobInputHttp) },
                    { "#Microsoft.Media.JobInputs", typeof(JobInputs) }
                }
            },
            // Layer
            new JsonConvertionRule
            {
                t = typeof(Layer),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.H264Layer ", typeof(H264Layer) },
                    { "#Microsoft.Media.JpgLayer", typeof(JpgLayer) },
                    { "#Microsoft.Media.PngLayer", typeof(PngLayer) }
                }
            },
            // Preset
            new JsonConvertionRule
            {
                t = typeof(Preset),
                convertRules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.BuiltInStandardEncoderPreset", typeof(BuiltInStandardEncoderPreset) },
                    { "#Microsoft.Media.AudioAnalyzerPreset", typeof(AudioAnalyzerPreset) },
                    { "#Microsoft.Media.VideoAnalyzerPreset", typeof(VideoAnalyzerPreset) },
                    { "#Microsoft.Media.StandardEncoderPreset", typeof(StandardEncoderPreset) }
                }
            }
        };

        public override bool CanConvert(Type objectType)
        {
            foreach (var c in conversions)
            {
                if (c.t.GetTypeInfo().Equals(objectType.GetTypeInfo())) return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null) return null;

            Type abstractType = objectType.GetTypeInfo();
            var tokens = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var targetODataType = tokens["@odata.type"].ToString();

            foreach (var c in conversions)
            {
                if (c.t.GetTypeInfo().Equals(abstractType))
                {
                    if (c.convertRules.ContainsKey(targetODataType))
                    {
                        Type targetType = c.convertRules[targetODataType];
                        return tokens.ToObject(targetType, serializer);
                    }
                    else
                    {
                        throw new NotSupportedException($"Type {targetODataType} is not supported in {nameof(c.t)}");
                    }
                }
            }

            throw new NotSupportedException($"Type {targetODataType} is not supported");
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class MediaServicesHelperTimeSpanJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (typeof(TimeSpan).GetTypeInfo().Equals(objectType.GetTypeInfo())) return true;
            else if (typeof(Nullable<TimeSpan>).GetTypeInfo().Equals(objectType.GetTypeInfo())) return true;
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null) return null;

            Type abstractType = objectType.GetTypeInfo();
            var token = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            string value = token.ToString();
            // ISO 8601 - PnYnMnDTnHnMnS
            if (value.StartsWith("P"))
            {
                return System.Xml.XmlConvert.ToTimeSpan(value);
            } else
            {
                return token.ToObject<TimeSpan>();
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}