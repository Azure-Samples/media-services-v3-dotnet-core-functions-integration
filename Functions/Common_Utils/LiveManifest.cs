// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Common_Utils
{
    public class LiveManifest
    {

        public static async Task<XDocument> TryToGetClientManifestContentAsABlobAsync(IAzureMediaServicesClient amsClient, string amsResourceGroup, string amsAccountName, string assetName)
        {
            // get the manifest
            ListContainerSasInput input = new()
            {
                Permissions = AssetContainerPermission.Read,
                ExpiryTime = DateTime.Now.AddMinutes(5).ToUniversalTime()
            };

            AssetContainerSas responseSas = await amsClient.Assets.ListContainerSasAsync(amsResourceGroup, amsAccountName, assetName, input.Permissions, input.ExpiryTime);

            string uploadSasUrl = responseSas.AssetContainerSasUrls.First();

            var sasUri = new Uri(uploadSasUrl);
            var container = new BlobContainerClient(sasUri);

            var blobs = new List<BlobItem>();
            await foreach (Azure.Page<BlobItem> page in container.GetBlobsAsync().AsPages()) // BlobTraits.All, BlobStates.All
            {
                blobs.AddRange(page.Values);
            }

            var ismc = blobs.Where(b => b.Properties.BlobType == BlobType.Block && b.Name.EndsWith(".ismc", StringComparison.OrdinalIgnoreCase));

            if (!ismc.Any())
            {
                throw new Exception("No ISMC file in asset.");
            }

            BlobClient blobClient = container.GetBlobClient(ismc.First().Name);

            var response = new BlobClient(blobClient.Uri).DownloadContent();

            return XDocument.Parse(response.Value.Content.ToString());
        }


        /// <summary>
        /// Parse the manifest data.
        /// It is recommended to call TryToGetClientManifestContentAsABlobAsync and TryToGetClientManifestContentUsingStreamingLocatorAsync to get the content
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public static ManifestTimingData GetManifestTimingData(XDocument manifest)
        {

            if (manifest == null)
            {
                throw new ArgumentNullException();
            }

            ManifestTimingData response = new() { IsLive = false, Error = false, TimestampOffset = 0, TimestampList = new List<ulong>(), DiscontinuityDetected = false };

            try
            {
                XElement smoothmedia = manifest.Element("SmoothStreamingMedia");
                IEnumerable<XElement> videotrack = smoothmedia.Elements("StreamIndex").Where(a => a.Attribute("Type").Value == "video");

                // TIMESCALE
                long? rootTimeScaleFromManifest = smoothmedia.Attribute("TimeScale").Value != null ? long.Parse(smoothmedia.Attribute("TimeScale").Value) : (long?)null;

                long? videoTimeScaleFromManifest = null;
                if (videotrack.FirstOrDefault().Attribute("TimeScale") != null) // there is timescale value in the video track. Let's take this one.
                {
                    videoTimeScaleFromManifest = long.Parse(videotrack.FirstOrDefault().Attribute("TimeScale").Value);
                }

                // by default, we use the timescale of the video track, except if there is no timescale. In that case, let's take the root one.
                long timescaleVideo = (long)(videoTimeScaleFromManifest ?? rootTimeScaleFromManifest);
                response.TimeScale = timescaleVideo;

                // DURATION
                string durationFromManifest = smoothmedia.Attribute("Duration").Value;
                ulong? overallDuration = null;
                if (durationFromManifest != null && rootTimeScaleFromManifest != null) // there is a duration value in the root (and a timescale). Let's take this one.
                {
                    var ratio = (double)rootTimeScaleFromManifest / (double)timescaleVideo;
                    overallDuration = (ulong?)(ulong.Parse(durationFromManifest) / ratio); // value with the timescale of the video track
                }

                // Timestamp offset
                if (videotrack.FirstOrDefault().Element("c").Attribute("t") != null)
                {
                    response.TimestampOffset = ulong.Parse(videotrack.FirstOrDefault().Element("c").Attribute("t").Value);
                }
                else
                {
                    response.TimestampOffset = 0; // no timestamp, so it should be 0
                }

                ulong totalDuration = 0;
                ulong durationPreviousChunk = 0;
                ulong durationChunk;
                int repeatChunk;
                foreach (XElement chunk in videotrack.Elements("c"))
                {
                    durationChunk = chunk.Attribute("d") != null ? ulong.Parse(chunk.Attribute("d").Value) : 0;
                    repeatChunk = chunk.Attribute("r") != null ? int.Parse(chunk.Attribute("r").Value) : 1;

                    if (chunk.Attribute("t") != null)
                    {
                        ulong tvalue = ulong.Parse(chunk.Attribute("t").Value);
                        response.TimestampList.Add(tvalue);
                        if (tvalue != response.TimestampOffset)
                        {
                            totalDuration = tvalue - response.TimestampOffset; // Discountinuity ? We calculate the duration from the offset
                            response.DiscontinuityDetected = true; // let's flag it
                        }
                    }
                    else
                    {
                        response.TimestampList.Add(response.TimestampList[^1] + durationPreviousChunk);
                    }

                    totalDuration += durationChunk * (ulong)repeatChunk;

                    for (int i = 1; i < repeatChunk; i++)
                    {
                        response.TimestampList.Add(response.TimestampList[^1] + durationChunk);
                    }

                    durationPreviousChunk = durationChunk;
                }
                response.TimestampEndLastChunk = response.TimestampList[^1] + durationPreviousChunk;

                if (smoothmedia.Attribute("IsLive") != null && smoothmedia.Attribute("IsLive").Value == "TRUE")
                { // Live asset.... No duration to read or it is always zero (but we can read scaling and compute duration)
                    response.IsLive = true;
                    response.AssetDuration = TimeSpan.FromSeconds(totalDuration / ((double)timescaleVideo));
                }
                else
                {
                    if (overallDuration != null & overallDuration > 0) // let's trust the duration property in the manifest
                    {
                        response.AssetDuration = TimeSpan.FromSeconds((ulong)overallDuration / ((double)timescaleVideo));

                    }
                    else // no trust
                    {
                        response.AssetDuration = TimeSpan.FromSeconds(totalDuration / ((double)timescaleVideo));
                    }
                }
            }
            catch
            {
                response.Error = true;
            }
            return response;
        }
                        
        /// <summary>
        /// Return the exact timespan on GOP
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        static public TimeSpan ReturnTimeSpanOnGOP(ManifestTimingData data, TimeSpan ts)
        {
            var response = ts;
            ulong timestamp = (ulong)(ts.TotalSeconds * data.TimeScale);

            int i = 0;
            foreach (var t in data.TimestampList)
            {
                if (t < timestamp && i < (data.TimestampList.Count - 1) && timestamp < data.TimestampList[i + 1])
                {
                    response = TimeSpan.FromSeconds((double)t / (double)data.TimeScale);
                    break;
                }
                i++;
            }
            return response;
        }
    }
}