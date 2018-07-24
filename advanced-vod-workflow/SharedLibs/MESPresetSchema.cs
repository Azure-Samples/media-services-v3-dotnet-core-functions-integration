//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//


using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;


namespace advanced_vod_functions_v3.SharedLibs
{
    public class MESPresetSchema
    {
        public string Version { get; set; }
        public Encoding[] Codecs { get; set; }
        public Output[] Outputs { get; set; }
        public Source[] Sources { get; set; }
    }
    public class Encoding
    {
        public string Label { get; set; }
    }
    public class EncodingVideo : Encoding
    {
        public TimeSpan KeyFrameInterval { get; set; }
        public string StretchMode { get; set; }
        public EncodingVideo() : base()
        {
            this.StretchMode = "AutoSize";
        }
    }
    public class EncodingAudio : Encoding
    {
    }
    public class H264Video : EncodingVideo
    {
        public bool TwoPass { get; set; }
        public bool SceneChangeDetection { get; set; }
        public string Complexity { get; set; }
        //public string SyncMode { get; set; }
        public H264Layer[] H264Layers { get; set; }
        public H264Video() : base()
        {
            // Default Values
            this.KeyFrameInterval = new TimeSpan(0, 0, 2); // 2 secs
            this.TwoPass = false;
            this.SceneChangeDetection = false;
            this.Complexity = "Balanced";
        }
    }
    public class H264Layer
    {
        public string Profile { get; set; }
        public string Level { get; set; }
        public int Bitrate { get; set; } // in kbps
        public int MaxBitrate { get; set; } // in kbps
        public TimeSpan BufferWindow { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public int BFrames { get; set; }
        public int ReferenceFrames { get; set; }
        public string EntropyMode { get; set; }
        public string FrameRate { get; set; }
        public bool AdaptiveBFrame { get; set; }
        public int Slices { get; set; }
        public string Label { get; set; }
        public H264Layer()
        {
            // Default Values
            this.Profile = "Auto";
            this.Level = "Auto";
            this.BufferWindow = new TimeSpan(0, 0, 5); // 5 secs
            this.ReferenceFrames = 3;
            this.EntropyMode = "Cabac";
            this.FrameRate = "0/1";
            this.Slices = 0;
        }
    }
    public class AACAudio : EncodingAudio
    {
        public string Profile { get; set; }
        public int Channels { get; set; }
        public int SamplingRate { get; set; }
        public int Bitrate { get; set; }
        public AACAudio() : base()
        {
            // Default Values
            this.Profile = "AACLC";
        }

    }
    public class ThumbnailImage : EncodingVideo
    {
        public string Start { get; set; }
        public string Step { get; set; }
        public string Range { get; set; }
        public ThumbnailImage() : base() { }
    }
    public class BmpImage : ThumbnailImage
    {
        public BmpLayer[] BmpLayers { get; set; }
        public BmpImage() : base() { }
    }
    public class JpgImage : ThumbnailImage
    {
        public JpgLayer[] JpgLayers { get; set; }
        public JpgImage() : base() { }
    }
    public class PngImage : ThumbnailImage
    {
        public PngLayer[] PngLayers { get; set; }
        public PngImage() : base() { }
    }
    public class ThumbnailImageLayer
    {
        public string Label { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }
    public class BmpLayer : ThumbnailImageLayer
    {
    }
    public class PngLayer : ThumbnailImageLayer
    {
    }
    public class JpgLayer : ThumbnailImageLayer
    {
        public int Quality { get; set; }
    }
    public class Output
    {
        public string FileName { get; set; }
        public Format Format { get; set; }
    }
    public class Format
    {
        public string Type { get; set; }
    }
    public class Source
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Filters Filters { get; set; }
    }
    public class Filters
    {
        // Need to be implemented
    }

    public class MESPresentSchemaJsonConverter : JsonConverter
    {
        static List<JsonObjectConversionRule> conversions = new List<JsonObjectConversionRule>
        {
            // Encoding
            new JsonObjectConversionRule
            {
                t = typeof(Encoding),
                rules = new Dictionary<string, Type>()
                {
                    { "H264Video", typeof(H264Video) },
                    { "AACAudio", typeof(AACAudio) },
                    { "BmpImage", typeof(BmpImage) },
                    { "JpgImage", typeof(JpgImage) },
                    { "PngImage", typeof(PngImage) }
                }
            },
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
            if (reader.TokenType == JsonToken.Null) return null;

            Type abstractType = objectType.GetTypeInfo();
            var tokens = Newtonsoft.Json.Linq.JToken.ReadFrom(reader);
            var targetODataType = tokens["Type"].ToString();

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
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
