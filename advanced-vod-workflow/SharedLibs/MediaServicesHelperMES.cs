//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

//using Microsoft.Azure.Management.Media.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace advanced_vod_functions_v3.SharedLibs
{
    public class MediaServicesHelperMES
    {
        public static Microsoft.Azure.Management.Media.Models.StandardEncoderPreset convertMESPreset(MESPresetSchema v2preset)
        {
            Microsoft.Azure.Management.Media.Models.StandardEncoderPreset v3preset = new Microsoft.Azure.Management.Media.Models.StandardEncoderPreset();
            v3preset.Codecs = new List<Microsoft.Azure.Management.Media.Models.Codec>();
            foreach (var s in v2preset.Codecs)
            {
                v3preset.Codecs.Add(convertMesPresetObeject(s));
            }
            v3preset.Formats = new List<Microsoft.Azure.Management.Media.Models.Format>();
            foreach (var s in v2preset.Outputs)
            {
                v3preset.Formats.Add(convertMesPresetObeject(s));
            }
            if (v2preset.Sources != null)
            {
                //v3preset.Filters = new Microsoft.Azure.Management.Media.Models.Filters();
            }
            return v3preset;
        }
        public static Microsoft.Azure.Management.Media.Models.Codec convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.Encoding s)
        {
            Type t = s.GetType();

            switch (t.Name)
            {
                case "AACAudio": return convertMesPresetObeject((AACAudio)s);
                case "H264Video": return convertMesPresetObeject((H264Video)s);
                case "JpgImage": return convertMesPresetObeject((JpgImage)s);
                case "PngImage": return convertMesPresetObeject((PngImage)s);
                case "BmpImage":
                default:
                    throw new NotSupportedException($"MES Encoding Class {t.Name} is not supported");
            }
        }
        public static Microsoft.Azure.Management.Media.Models.AacAudio convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.AACAudio s)
        {
            Microsoft.Azure.Management.Media.Models.AacAudio d = new Microsoft.Azure.Management.Media.Models.AacAudio();
            d.Bitrate = s.Bitrate * 1000;
            d.Channels = s.Channels;
            d.Label = s.Label;
            d.Profile = (Microsoft.Azure.Management.Media.Models.AacAudioProfile)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.AacAudioProfile), s.Profile);
            d.SamplingRate = s.SamplingRate;
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.H264Video convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.H264Video s)
        {
            Microsoft.Azure.Management.Media.Models.H264Video d = new Microsoft.Azure.Management.Media.Models.H264Video();
            d.Complexity = (Microsoft.Azure.Management.Media.Models.H264Complexity)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.H264Complexity), s.Complexity);
            d.KeyFrameInterval = s.KeyFrameInterval;
            d.Label = s.Label;
            d.SceneChangeDetection = s.SceneChangeDetection;
            d.StretchMode = (Microsoft.Azure.Management.Media.Models.StretchMode)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.StretchMode), s.StretchMode);
            if (s.H264Layers != null)
            {
                d.Layers = new List<Microsoft.Azure.Management.Media.Models.H264Layer>();
                foreach (var v in s.H264Layers)
                {
                    H264Layer layer = (H264Layer)v;
                    d.Layers.Add(convertMesPresetObeject(layer));
                }
            }
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.JpgImage convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.JpgImage s)
        {
            Microsoft.Azure.Management.Media.Models.JpgImage d = new Microsoft.Azure.Management.Media.Models.JpgImage();
            d.KeyFrameInterval = s.KeyFrameInterval;
            d.Label = s.Label;
            d.Range = s.Range;
            d.Start = s.Start;
            d.Step = s.Step;
            d.StretchMode = (Microsoft.Azure.Management.Media.Models.StretchMode)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.StretchMode), s.StretchMode);
            if (s.JpgLayers != null)
            {
                d.Layers = new List<Microsoft.Azure.Management.Media.Models.JpgLayer>();
                foreach (var v in s.JpgLayers)
                {
                    JpgLayer layer = (JpgLayer)v;
                    d.Layers.Add(convertMesPresetObeject(layer));
                }
            }
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.PngImage convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.PngImage s)
        {
            Microsoft.Azure.Management.Media.Models.PngImage d = new Microsoft.Azure.Management.Media.Models.PngImage();
            d.KeyFrameInterval = s.KeyFrameInterval;
            d.Label = s.Label;
            d.Range = s.Range;
            d.Start = s.Start;
            d.Step = s.Step;
            d.StretchMode = (Microsoft.Azure.Management.Media.Models.StretchMode)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.StretchMode), s.StretchMode);
            if (s.PngLayers != null)
            {
                d.Layers = new List<Microsoft.Azure.Management.Media.Models.PngLayer>();
                foreach (var v in s.PngLayers)
                {
                    PngLayer layer = (PngLayer)v;
                    d.Layers.Add(convertMesPresetObeject(layer));
                }
            }
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.H264Layer convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.H264Layer s)
        {
            Microsoft.Azure.Management.Media.Models.H264Layer d = new Microsoft.Azure.Management.Media.Models.H264Layer();
            d.AdaptiveBFrame = s.AdaptiveBFrame;
            d.BFrames = s.BFrames;
            d.Bitrate = s.Bitrate * 1000;
            d.BufferWindow = s.BufferWindow;
            d.EntropyMode = (Microsoft.Azure.Management.Media.Models.EntropyMode)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.EntropyMode), s.EntropyMode);
            d.FrameRate = s.FrameRate;
            d.Height = s.Height;
            d.Label = s.Label;
            d.Level = s.Level;
            d.MaxBitrate = s.MaxBitrate * 1000;
            d.Profile = (Microsoft.Azure.Management.Media.Models.H264VideoProfile)convertMediaModelsField(typeof(Microsoft.Azure.Management.Media.Models.H264VideoProfile), s.Profile);
            d.ReferenceFrames = s.ReferenceFrames;
            d.Slices = s.Slices;
            d.Width = s.Width;
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.JpgLayer convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.JpgLayer s)
        {
            Microsoft.Azure.Management.Media.Models.JpgLayer d = new Microsoft.Azure.Management.Media.Models.JpgLayer();
            d.Height = s.Height;
            d.Label = s.Label;
            d.Quality = s.Quality;
            d.Width = s.Width;
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.PngLayer convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.PngLayer s)
        {
            Microsoft.Azure.Management.Media.Models.PngLayer d = new Microsoft.Azure.Management.Media.Models.PngLayer();
            d.Height = s.Height;
            d.Label = s.Label;
            d.Width = s.Width;
            return d;
        }
        public static Microsoft.Azure.Management.Media.Models.Format convertMesPresetObeject(advanced_vod_functions_v3.SharedLibs.Output s)
        {
            Microsoft.Azure.Management.Media.Models.Format d = null;
            switch (s.Format.Type)
            {
                case "MP4Format":
                    d = new Microsoft.Azure.Management.Media.Models.Mp4Format(s.FileName, new List<Microsoft.Azure.Management.Media.Models.OutputFile>());
                    d.FilenamePattern = s.FileName;
                    break;
                case "JpgFormat":
                    d = new Microsoft.Azure.Management.Media.Models.JpgFormat();
                    d.FilenamePattern = s.FileName;
                    break;
                case "PngFormat":
                    d = new Microsoft.Azure.Management.Media.Models.PngFormat();
                    d.FilenamePattern = s.FileName;
                    break;
                case "BmpFormat":
                default:
                    throw new NotSupportedException($"MES Output Format Type {s.Format.Type} is not supported");
            }
            return d;
        }

        ///  Private Methods
        private class MediaModelsFieldConversion
        {
            public Type t;
            public Dictionary<string, Object> rules;
        }
        static List<MediaModelsFieldConversion> fieldConversions = new List<MediaModelsFieldConversion>
        {
            new MediaModelsFieldConversion
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.AacAudioProfile),
                rules = new Dictionary<string, Object>()
                {
                    { "AACLC", Microsoft.Azure.Management.Media.Models.AacAudioProfile.AacLc },
                    { "HEAACV1", Microsoft.Azure.Management.Media.Models.AacAudioProfile.HeAacV1 },
                    { "HEAACV2", Microsoft.Azure.Management.Media.Models.AacAudioProfile.HeAacV2 }
                }
            },
            new MediaModelsFieldConversion
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.EntropyMode),
                rules = new Dictionary<string, Object>()
                {
                    { "Cabac", Microsoft.Azure.Management.Media.Models.EntropyMode.Cabac },
                    { "Cavlc", Microsoft.Azure.Management.Media.Models.EntropyMode.Cavlc }
                }
            },
            new MediaModelsFieldConversion
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.H264Complexity),
                rules = new Dictionary<string, Object>()
                {
                    { "Balanced", Microsoft.Azure.Management.Media.Models.H264Complexity.Balanced },
                    { "Quality", Microsoft.Azure.Management.Media.Models.H264Complexity.Quality },
                    { "Speed", Microsoft.Azure.Management.Media.Models.H264Complexity.Speed }
                }
            },
            new MediaModelsFieldConversion
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.H264VideoProfile),
                rules = new Dictionary<string, Object>()
                {
                    { "Auto", Microsoft.Azure.Management.Media.Models.H264VideoProfile.Auto },
                    { "Baseline", Microsoft.Azure.Management.Media.Models.H264VideoProfile.Baseline },
                    { "High", Microsoft.Azure.Management.Media.Models.H264VideoProfile.High },
                    { "High422", Microsoft.Azure.Management.Media.Models.H264VideoProfile.High422 },
                    { "High444", Microsoft.Azure.Management.Media.Models.H264VideoProfile.High444 },
                    { "Main", Microsoft.Azure.Management.Media.Models.H264VideoProfile.Main }
                }
            },
            new MediaModelsFieldConversion
            {
                t = typeof(Microsoft.Azure.Management.Media.Models.StretchMode),
                rules = new Dictionary<string, Object>()
                {
                    { "AutoFit", Microsoft.Azure.Management.Media.Models.StretchMode.AutoFit },
                    { "AutoSize", Microsoft.Azure.Management.Media.Models.StretchMode.AutoSize },
                    { "None", Microsoft.Azure.Management.Media.Models.StretchMode.None }
                }
            }
        };
        private static Object convertMediaModelsField(Type t, string s)
        {
            foreach (var c in fieldConversions)
            {
                if (c.t.GetTypeInfo().Equals(t.GetTypeInfo()))
                {
                    if (c.rules.ContainsKey(s))
                    {
                        return c.rules[s];
                    }
                    else
                    {
                        throw new NotSupportedException($"Source Value {s} is not supported in {nameof(c.t)}");
                    }
                }
            }
            throw new NotSupportedException($"Target Type {t} is not supported");
        }
    }
}
