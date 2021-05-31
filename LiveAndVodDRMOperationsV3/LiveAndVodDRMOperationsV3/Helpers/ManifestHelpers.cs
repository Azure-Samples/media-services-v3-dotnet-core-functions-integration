using Microsoft.Azure.Management.Media.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using LiveDRMOperationsV3.Models;
using Microsoft.Azure.Storage.Blob;

namespace LiveDRMOperationsV3.Helpers
{
    public class ManifestGenerated
    {
        public string FileName;
        public string Content;

    }
    class ManifestHelpers
    {
        public async static Task<ManifestGenerated> LoadAndUpdateManifestTemplate(Asset asset, CloudBlobContainer container, Microsoft.Azure.WebJobs.ExecutionContext execContext, string manifestFileName = null)
        {
            List<IListBlobItem> blobsresult = new List<IListBlobItem>();
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, continuationToken, null, null);
                continuationToken = response.ContinuationToken;
                blobsresult.AddRange(response.Results);
            }
            while (continuationToken != null);

            var blobs = blobsresult.Where(c => c.GetType() == typeof(CloudBlockBlob)).Select(c => c as CloudBlockBlob);

            var mp4AssetFiles = blobs.Where(f => f.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)).ToArray();
            var m4aAssetFiles = blobs.Where(f => f.Name.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase)).ToArray();
            var mediaAssetFiles = blobs.Where(f => f.Name.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || f.Name.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (mediaAssetFiles.Count() != 0)
            {
                // Prepare the manifest
                string mp4fileuniqueaudio = null;

                // let's load the manifest template
                string manifestPath = Path.Combine(System.IO.Directory.GetParent(execContext.FunctionDirectory).FullName, "ManifestTemplate", "manifest.ism");

                XDocument doc = XDocument.Load(manifestPath);

                XNamespace ns = "http://www.w3.org/2001/SMIL20/Language";

                var bodyxml = doc.Element(ns + "smil");
                var body2 = bodyxml.Element(ns + "body");

                var switchxml = body2.Element(ns + "switch");

                // audio tracks (m4a)
                foreach (var file in m4aAssetFiles)
                {
                    switchxml.Add(new XElement(ns + "audio", new XAttribute("src", file.Name), new XAttribute("title", Path.GetFileNameWithoutExtension(file.Name))));
                }

                if (m4aAssetFiles.Count() == 0)
                {
                    // audio track(s)
                    var mp4AudioAssetFilesName = mp4AssetFiles.Where(f =>
                                                               (f.Name.ToLower().Contains("audio") && !f.Name.ToLower().Contains("video"))
                                                               ||
                                                               (f.Name.ToLower().Contains("aac") && !f.Name.ToLower().Contains("h264"))
                                                               );

                    var mp4AudioAssetFilesSize = mp4AssetFiles.OrderBy(f => f.Properties.Length);

                    string mp4fileaudio = (mp4AudioAssetFilesName.Count() == 1) ? mp4AudioAssetFilesName.FirstOrDefault().Name : mp4AudioAssetFilesSize.FirstOrDefault().Name; // if there is one file with audio or AAC in the name then let's use it for the audio track
                    switchxml.Add(new XElement(ns + "audio", new XAttribute("src", mp4fileaudio), new XAttribute("title", "audioname")));

                    if (mp4AudioAssetFilesName.Count() == 1 && mediaAssetFiles.Count() > 1) //looks like there is one audio file and dome other video files
                    {
                        mp4fileuniqueaudio = mp4fileaudio;
                    }
                }

                // video tracks
                foreach (var file in mp4AssetFiles)
                {
                    if (file.Name != mp4fileuniqueaudio) // we don't put the unique audio file as a video track
                    {
                        switchxml.Add(new XElement(ns + "video", new XAttribute("src", file.Name)));
                    }
                }

                // manifest filename
                if (manifestFileName == null)
                {
                    manifestFileName = CommonPrefix(mediaAssetFiles.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToArray());
                    if (string.IsNullOrEmpty(manifestFileName))
                    {
                        manifestFileName = "manifest";
                    }
                    else if (manifestFileName.EndsWith("_") && manifestFileName.Length > 1) // i string ends with "_", let's remove it
                    {
                        manifestFileName = manifestFileName.Substring(0, manifestFileName.Length - 1);
                    }
                    manifestFileName = manifestFileName + ".ism";
                }
                else if (!manifestFileName.ToLower().EndsWith(".ism"))
                    manifestFileName = manifestFileName + ".ism";



                return new ManifestGenerated() { Content = doc.Declaration.ToString() + Environment.NewLine + doc.ToString(), FileName = manifestFileName };

            }
            else
            {
                return new ManifestGenerated() { Content = null, FileName = string.Empty }; // no mp4 in asset
            }
        }

        public async static Task<ManifestGenerated> LoadAndUpdateManifestTemplateUsingSemaphore(VodSemaphore semaphore, Microsoft.Azure.WebJobs.ExecutionContext execContext, string manifestFileName = null)
        {

            var mediaAssetFiles = semaphore.Files.Where(f => f.ContainsVideo || f.ContainsAudio);
            var audioFiles = semaphore.Files.Where(f => f.ContainsAudio);
            var videoFiles = semaphore.Files.Where(f => f.ContainsVideo);

            if (mediaAssetFiles.Count() != 0)
            {
                // let's load the manifest template
                string manifestPath = Path.Combine(System.IO.Directory.GetParent(execContext.FunctionDirectory).FullName, "ManifestTemplate", "manifest.ism");

                XDocument doc = XDocument.Load(manifestPath);

                XNamespace ns = "http://www.w3.org/2001/SMIL20/Language";

                var bodyxml = doc.Element(ns + "smil");
                var body2 = bodyxml.Element(ns + "body");

                var switchxml = body2.Element(ns + "switch");

                // audio tracks
                foreach (var file in audioFiles)
                {
                    var myListAt = new List<XAttribute>() { new XAttribute("src", file.FileName) };
                    if (file.AudioLanguage != null)
                        myListAt.Add(new XAttribute("systemLanguage", file.AudioLanguage));
                    /*
                     * // No need as it is recommended to use trackName for audio
                    if (file.AudioTitle != null)
                        myListAt.Add(new XAttribute("title", file.AudioTitle));
                        */

                    var myListElem = new List<XElement>();
                    if (file.AudioTrackName != null)
                        myListElem.Add(new XElement(ns + "param", new List<XAttribute> { new XAttribute("name", "trackName"), new XAttribute("value", file.AudioTrackName) }));
                    if (file.AudioAccessibility != null)
                        myListElem.Add(new XElement(ns + "param", new List<XAttribute> { new XAttribute("name", "accessibility"), new XAttribute("value", file.AudioAccessibility) }));
                    if (file.AudioRole != null)
                        myListElem.Add(new XElement(ns + "param", new List<XAttribute> { new XAttribute("name", "role"), new XAttribute("value", file.AudioRole) }));

                    var audioElement = new XElement(ns + "audio", myListAt);
                    audioElement.Add(myListElem);
                    switchxml.Add(audioElement);
                }


                // video tracks
                foreach (var file in videoFiles)
                {
                    switchxml.Add(new XElement(ns + "video", new XAttribute("src", file.FileName)));
                }

                // manifest filename
                if (manifestFileName == null)
                {
                    manifestFileName = CommonPrefix(mediaAssetFiles.Select(f => Path.GetFileNameWithoutExtension(f.FileName)).ToArray());
                    if (string.IsNullOrEmpty(manifestFileName))
                    {
                        manifestFileName = "manifest";
                    }
                    else if (manifestFileName.EndsWith("_") && manifestFileName.Length > 1) // i string ends with "_", let's remove it
                    {
                        manifestFileName = manifestFileName.Substring(0, manifestFileName.Length - 1);
                    }
                    manifestFileName = manifestFileName + ".ism";
                }
                else if (!manifestFileName.ToLower().EndsWith(".ism"))
                    manifestFileName = manifestFileName + ".ism";

                return new ManifestGenerated() { Content = doc.Declaration.ToString() + Environment.NewLine + doc.ToString(), FileName = manifestFileName };

            }
            else
            {
                return new ManifestGenerated() { Content = null, FileName = string.Empty }; // no mp4 in asset
            }
        }


        private static string CommonPrefix(string[] ss)
        {
            if (ss.Length == 0)
            {
                return "";
            }

            if (ss.Length == 1)
            {
                return ss[0];
            }

            int prefixLength = 0;

            foreach (char c in ss[0])
            {
                foreach (string s in ss)
                {
                    if (s.Length <= prefixLength || s[prefixLength] != c)
                    {
                        return ss[0].Substring(0, prefixLength);
                    }
                }
                prefixLength++;
            }

            return ss[0]; // all strings identical
        }


        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
