// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using System;
using System.Threading.Tasks;

namespace Common_Utils
{
    public class TransformUtils
    {
        /// <summary>
        /// If the specified transform exists, return that transform. If the it does not
        /// exist, creates a new transform with the specified output. In this case, the
        /// output is set to encode a video using a predefined preset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The transform name.</param>
        /// <param name="builtInPreset">The built in standard encoder preset to use if the transform is created.</param>
        /// <returns></returns>
        public static async Task<Transform> CreateEncodingTransform(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName, string builtInPreset)
        {
            // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = client.Transforms.Get(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                Console.WriteLine("Creating the transform...");
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                  new TransformOutput(
                        new BuiltInStandardEncoderPreset()
                        {
                            // Pass the buildin preset name.
                            PresetName = builtInPreset
                        },
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = $"A encoding transform using {builtInPreset} preset";

                // Create the Transform with the outputs defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, outputs, description);
            }

            return transform;
        }


        /// <summary>
        /// If the specified transform exists, return that transform. If the it does not
        /// exist, creates a new transform with the specified output. In this case, the
        /// output is set to encode a video using a custom preset.
        /// </summary>
        /// <param name="client">The Media Services client.</param>
        /// <param name="resourceGroupName">The name of the resource group within the Azure subscription.</param>
        /// <param name="accountName"> The Media Services account name.</param>
        /// <param name="transformName">The transform name.</param>
        /// <returns></returns>
        public static async Task<Transform> CreateSubclipTransform(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName)
        {
            // Does a transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            Transform transform = client.Transforms.Get(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                Console.WriteLine("Creating a custom transform...");
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                  new TransformOutput(
                        CopyOnlyPreset(),
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = "A subclip transform with top bitrate archiving";
                // Create the custom Transform with the outputs defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, outputs, description);
            }

            return transform;
        }


        /// <summary>
        /// Preset for subclipping. It archives the top bitrate as a single MP4 file in a new asset.
        /// </summary>
        /// <returns></returns>
        public static StandardEncoderPreset CopyOnlyPreset()
        {
            return new StandardEncoderPreset(
       codecs: new Codec[]
       {
                        // Add an Audio layer for the audio copy
                        new CopyAudio(),                 
                        // Next, add a Video for the video copy
                       new CopyVideo()
        },
         // Specify the format for the output files - one for video+audio, and another for the thumbnails
         formats: new Format[]
         {
                        new Mp4Format(
                            filenamePattern:"Archive-{Basename}{Extension}"
                        )
         });
        }
    }
}