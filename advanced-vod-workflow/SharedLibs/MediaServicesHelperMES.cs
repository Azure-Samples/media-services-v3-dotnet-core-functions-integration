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
    public class MediaServicesHelperMES
    {

    }

#if false
    public class PresetConverter<Preset> : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return typeof(Preset).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
            {
                return null;
            }

            var tokens = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var targetType = tokens["@odata.type"].ToString();

            switch (targetType)
            {
                case "#Microsoft.Media.BuiltInStandardEncoderPreset":
                    return tokens.ToObject<BuiltInStandardEncoderPreset>();
                case "#Microsoft.Media.AudioAnalyzerPreset":
                    return tokens.ToObject<AudioAnalyzerPreset>();
                case "#Microsoft.Media.VideoAnalyzerPreset":
                    return tokens.ToObject<VideoAnalyzerPreset>();
                case "#Microsoft.Media.StandardEncoderPreset":
                    //return tokens.ToObject<StandardEncoderPreset>();
                    StandardEncoderPreset r = new StandardEncoderPreset();
                    JsonConverter[] jsonConverters = { new CodecConverter<Codec>(), new FormatConverter<Format>() };
                    //r.Filters = tokens["filters"].ToObject<>;
                    r.Codecs = JsonConvert.DeserializeObject<List<Codec>>(tokens["codecs"].ToString(), jsonConverters);
                    r.Formats = JsonConvert.DeserializeObject<List<Format>>(tokens["formats"].ToString(), jsonConverters);
                    return r;
                default:
                    throw new NotSupportedException($"Type {targetType} is not supported in {nameof(Codec)}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    public class CodecConverter<Codec> : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return typeof(Codec).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
            {
                return null;
            }

            var tokens = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var targetType = tokens["@odata.type"].ToString();

            switch (targetType)
            {
                case "#Microsoft.Media.H264Video":
                    return tokens.ToObject<H264Video>();
                case "#Microsoft.Media.AacAudio":
                    return tokens.ToObject<AacAudio>();
                case "#Microsoft.Media.JpgImage":
                    return tokens.ToObject<JpgImage>();
                case "#Microsoft.Media.PngImage":
                    return tokens.ToObject<PngImage>();
                default:
                    throw new NotSupportedException($"Type {targetType} is not supported in {nameof(Codec)}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

    public class FormatConverter<Format> : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return typeof(Format).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
            {
                return null;
            }

            var tokens = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var targetType = tokens["@odata.type"].ToString();

            switch (targetType)
            {
                case "#Microsoft.Media.Mp4Format":
                    return tokens.ToObject<Mp4Format>();
                case "#Microsoft.Media.TransportStreamFormat":
                    return tokens.ToObject<TransportStreamFormat>();
                case "#Microsoft.Media.JpgFormat":
                    return tokens.ToObject<JpgFormat>();
                case "#Microsoft.Media.PngFormat":
                    return tokens.ToObject<PngFormat>();
                default:
                    throw new NotSupportedException($"Type {targetType} is not supported in {nameof(Codec)}");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
#endif
}
