//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace Common_Utils
{
    public class JsonObjectConversionRule
    {
        public Type t;
        public Dictionary<string, Type> rules;
    }

    abstract public class MediaServicesHelperJsonConverter : JsonConverter
    {
        protected static List<JsonObjectConversionRule> conversions = new List<JsonObjectConversionRule>
        {
            // Codec
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.Codec),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.AacAudio", typeof(Microsoft.Azure.Management.Media.Models.AacAudio) },
                    { "#Microsoft.Media.CopyAudio", typeof(Microsoft.Azure.Management.Media.Models.CopyAudio) },
                    { "#Microsoft.Media.CopyVideo", typeof(Microsoft.Azure.Management.Media.Models.CopyVideo) },
                    { "#Microsoft.Media.H264Video", typeof(Microsoft.Azure.Management.Media.Models.H264Video) },
                    { "#Microsoft.Media.JpgImage", typeof(Microsoft.Azure.Management.Media.Models.JpgImage) },
                    { "#Microsoft.Media.PngImage", typeof(Microsoft.Azure.Management.Media.Models.PngImage) }
                }
            },
            // ContentKeyPolicyConfiguration
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyConfiguration),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyClearKeyConfiguration", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyClearKeyConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyFairPlayConfiguration", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyFairPlayConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyConfiguration", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyPlayReadyConfiguration) },
                    { "#Microsoft.Media.ContentKeyPolicyWidevineConfiguration", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyWidevineConfiguration) }
                }
            },
            // ContentKeyPolicyRestriction
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyRestriction),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyOpenRestriction", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyOpenRestriction) },
                    { "#Microsoft.Media.ContentKeyPolicyTokenRestriction", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyTokenRestriction) }
                }
            },
            // ContentKeyPolicyRestrictionTokenKey
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyRestrictionTokenKey),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyRsaTokenKey", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyRsaTokenKey) },
                    { "#Microsoft.Media.ContentKeyPolicySymmetricTokenKey", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicySymmetricTokenKey) },
                    { "#Microsoft.Media.ContentKeyPolicyX509CertificateTokenKey", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyX509CertificateTokenKey) }
                }
            },
            // ContentKeyPolicyPlayReadyContentKeyLocation
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyPlayReadyContentKeyLocation),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader) },
                    { "#Microsoft.Media.ContentKeyPolicyPlayReadyContentEncryptionKeyFromKeyIdentifier", typeof(Microsoft.Azure.Management.Media.Models.ContentKeyPolicyPlayReadyContentEncryptionKeyFromKeyIdentifier) }
                }
            },
            // Format
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.Format),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.Mp4Format", typeof(Microsoft.Azure.Management.Media.Models.Mp4Format) },
                    { "#Microsoft.Media.TransportStreamFormat", typeof(Microsoft.Azure.Management.Media.Models.TransportStreamFormat) },
                    { "#Microsoft.Media.JpgFormat", typeof(Microsoft.Azure.Management.Media.Models.JpgFormat) },
                    { "#Microsoft.Media.PngFormat", typeof(Microsoft.Azure.Management.Media.Models.PngFormat) }
                }
            },
            // JobInput
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.JobInput),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.JobInputAsset ", typeof(Microsoft.Azure.Management.Media.Models.JobInputAsset) },
                    { "#Microsoft.Media.JobInputHttp", typeof(Microsoft.Azure.Management.Media.Models.JobInputHttp) },
                    { "#Microsoft.Media.JobInputs", typeof(Microsoft.Azure.Management.Media.Models.JobInputs) }
                }
            },
            // JobOutput
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.JobOutput),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.JobOutputAsset ", typeof(Microsoft.Azure.Management.Media.Models.JobOutputAsset) }
                }
            },
            // Layer
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.Layer),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.H264Layer ", typeof(Microsoft.Azure.Management.Media.Models.H264Layer) },
                    { "#Microsoft.Media.JpgLayer", typeof(Microsoft.Azure.Management.Media.Models.JpgLayer) },
                    { "#Microsoft.Media.PngLayer", typeof(Microsoft.Azure.Management.Media.Models.PngLayer) }
                }
            },
            // Overlay
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.Overlay),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.AudioOverlay", typeof(Microsoft.Azure.Management.Media.Models.AudioOverlay) },
                    { "#Microsoft.Media.VideoOverlay", typeof(Microsoft.Azure.Management.Media.Models.VideoOverlay) }
                }
            },
            // Preset
            new JsonObjectConversionRule
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.Preset),
                rules = new Dictionary<string, Type>()
                {
                    { "#Microsoft.Media.BuiltInStandardEncoderPreset", typeof(Microsoft.Azure.Management.Media.Models.BuiltInStandardEncoderPreset) },
                    { "#Microsoft.Media.AudioAnalyzerPreset", typeof(Microsoft.Azure.Management.Media.Models.AudioAnalyzerPreset) },
                    { "#Microsoft.Media.VideoAnalyzerPreset", typeof(Microsoft.Azure.Management.Media.Models.VideoAnalyzerPreset) },
                    { "#Microsoft.Media.StandardEncoderPreset", typeof(Microsoft.Azure.Management.Media.Models.StandardEncoderPreset) }
                }
            }
        };
        public override bool CanConvert(Type objectType) { throw new NotImplementedException(); }
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { throw new NotImplementedException(); }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { throw new NotImplementedException(); }
    }
    public class MediaServicesHelperJsonReader : MediaServicesHelperJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            foreach (var c in conversions)
            {
                if (c.t.GetTypeInfo().Equals(objectType.GetTypeInfo())) return true;
            }
            return false;
        }
        public override bool CanRead => true;
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
                    if (c.rules.ContainsKey(targetODataType))
                    {
                        Type targetType = c.rules[targetODataType];
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
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { throw new NotImplementedException(); }
    }
    public class MediaServicesHelperJsonWriter : MediaServicesHelperJsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            foreach (var c in conversions)
            {
                if (objectType == c.t || objectType.IsSubclassOf(c.t)) return true;
            }
            return false;
        }
        public override bool CanRead => false;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) { throw new NotImplementedException(); }
        public override bool CanWrite => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken tokens = JToken.FromObject(value);
            Type t = value.GetType();

            string odata = null;
            foreach (var c in conversions)
            {
                if (c.t.GetTypeInfo().IsAssignableFrom(t))
                {
                    foreach (var r in c.rules)
                    {
                        Type targetType = r.Value;
                        if (t.Equals(targetType))
                        {
                            odata = r.Key;
                        }
                    }
                }
            }

            PropertyInfo[] props = t.GetProperties();
            JObject o = new JObject();
            if (odata != null)
            {
                int count = 0;
                if (!t.IsSubclassOf(typeof(Microsoft.Azure.Management.Media.Models.Layer)))
                    o.AddFirst(new JProperty("@odata.type", odata));
                foreach (JToken child in tokens.Children())
                {
                    PropertyInfo p = props[count];
                    object pv = p.GetValue(value, null);
                    if (pv != null)
                        o.Add(child.Path, JToken.FromObject(pv, serializer));
                    else
                        o.Add(child.Path, null);
                    count++;
                }
                o.WriteTo(writer);
            }
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

        public override bool CanRead => true;
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
            }
            else
            {
                return token.ToObject<TimeSpan>();
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite => true;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken token = JToken.FromObject(value);
            TimeSpan ts = (TimeSpan)token;
            if (ts == new TimeSpan())
            {
                Nullable<TimeSpan> obj = null;
                writer.WriteValue(obj);
            }
            else
            {
                string hours = (ts.Hours != 0) ? ts.Hours.ToString() + "H" : "";
                string mins = (ts.Minutes != 0) ? ts.Minutes.ToString() + "M" : "";
                string secs = (ts.Seconds != 0) ? ts.Seconds.ToString() + "S" : "";
                string tsString = "PT" + hours + mins + secs;
                JToken o = JToken.FromObject(tsString);
                o.WriteTo(writer);
            }
        }
    }
}