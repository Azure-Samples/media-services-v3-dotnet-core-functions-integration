//
// Azure Media Services REST API v3 Functions
//
// CreatePlayReadyLicenseTemplate - This function creates a PlayReady License Template JSON data.
//
/*
```c#
Input:
    {
        // A flag indicating whether test devices can use the license.
        // Allowed values: true, false.
        // Default value: false.
        "allowTestDevices": false,

        // The license type.
        // Allowed values: 'NonPersistent', 'Persistent'.
        // Default value: 'NonPersistent'.
        "licenseType": "NonPersistent",

        // The content key location.
        // Allowed values: 'ContentEncryptionKeyFromHeader', 'ContentEncryptionKeyFromKeyIdentifier'.
        // Default value: 'ContentEncryptionKeyFromHeader'.
        "contentKeyLocation": "ContentEncryptionKeyFromHeader",

        // The PlayReady content type.
        // Allowed values: 'Unspecified', 'UltraVioletDownload', 'UltraVioletStreaming'.
        // Default value: 'Unspecified'
        "contentType": "UltraVioletStreaming",

        // The begin date (Y-m-d'T'H:M:S'Z') of license
        "beginDate": "2018-01-01T00:00Z",

        // The expiration date (Y-m-d'T'H:M:S'Z') of license.
        "expirationDate": "2018-12-31T23:59Z",

        // The relative begin date of license.
        "relativeBeginDate": "PT10H",

        // The relative expiration date of license.
        "relativeExpirationDate": "P30D",

        // The grace period of license.
        "gracePeriod": "PT5S",

        // PlayRight
        // Enables the Image Constraint For Analog Component Video Restriction in the license.
        // Allowed values: true, false.
        // Default value: false.
        "digitalVideoOnlyContentRestriction": false,
        // Enables the Image Constraint For Analog Component Video Restriction in the license.
        // Allowed values: true, false.
        // Default value: false.
        "imageConstraintForAnalogComponentVideoRestriction": false,
        // Enables the Image Constraint For Analog Component Video Restriction in the license.
        // Allowed values: true, false.
        // Default value: false.
        "imageConstraintForAnalogComputerMonitorRestriction": false,
        // Configures Unknown output handling settings of the license.
        // Allowed values: 'NotAllowed', 'Allowed', 'AllowedWithVideoConstriction'.
        // Default value: 'NotAllowed'.
        "allowPassingVideoContentToUnknownOutput": "NotAllowed",
        // The amount of time that the license is valid after the license is first used to play content.
        "firstPlayExpiration": "PT60M",
        // Configures the Serial Copy Management System (SCMS) in the license.
        // Must be between 0 and 3 inclusive.
        "scmsRestriction": 0,
        // Configures Automatic Gain Control (AGC) and Color Stripe in the license.
        // Must be between 0 and 3 inclusive.
        "agcAndColorStripeRestriction": 0,
        // Configures the Explicit Analog Television Output Restriction in the license.
        // Configuration data must be between 0 and 3 inclusive.
        "explicitAnalogTelevisionOutputRestriction": 0,
        // Configures the Explicit Analog Television Output Restriction in the license.
        // Allowed values: true, false.
        // Default value: false.
        "explicitAnalogTelevisionOutputRestrictionBestEffort": false,

        // Output Protection Level
        // Please see the document: https://docs.microsoft.com/en-us/playready/overview/output-protection-levels
        // Specifies the output protection level for uncompressed digital video.
        // Allowed Values: 100, 250, 270, 300.
        "uncompressedDigitalVideoOpl": 100,
        // Specifies the output protection level for compressed digital video.
        // Allowed Values: 400, 500.
        "compressedDigitalVideoOpl": 400,
        // Specifies the output protection level for compressed digital audio.
        // Allowed Values: 100, 150, 200.
        "analogVideoOpl": 100,
        // Specifies the output protection level for compressed digital audio.
        // Allowed Values: 100, 150, 200, 250, 300.
        "compressedDigitalAudioOpl": 100,
        // Specifies the output protection level for uncompressed digital audio.
        // Allowed Values: 100, 150, 200, 250, 300.
        "uncompressedDigitalAudioOpl": 100,

        // PlayReady License Template JSONs
        "playReadyLicenses": [ { ... } ]
    }
Output:
    {
        // The JSON string of PlayReady License Templates.
        "playReadyLicenses": [ { ... }, { ... } ]
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


namespace advanced_vod_functions_v3.Functions
{
    public static class CreatePlayReadyLicenseTemplate
    {
        [FunctionName("CreatePlayReadyLicenseTemplate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AMS v3 Function - CreatePlayReadyLicenseTemplate was triggered!");

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

                if (data.playReadyLicenses != null)
                    licenseTemplates = JsonConvert.DeserializeObject<List<ContentKeyPolicyPlayReadyLicense>>(data.playReadyLicenses.ToString(), jsonReaders);

                ContentKeyPolicyPlayReadyLicense licenseTemplate = new ContentKeyPolicyPlayReadyLicense();
                licenseTemplate.PlayRight = new ContentKeyPolicyPlayReadyPlayRight();

                // default value settings
                licenseTemplate.ContentKeyLocation = new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader();
                licenseTemplate.ContentType = ContentKeyPolicyPlayReadyContentType.Unspecified;
                licenseTemplate.LicenseType = ContentKeyPolicyPlayReadyLicenseType.NonPersistent;
                licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.NotAllowed;

                if (data.allowTestDevices != null) licenseTemplate.AllowTestDevices = data.allowTestDevices;
                if (data.licenseType != null)
                {
                    switch (data.licenseType)
                    {
                        case "NonPersistent":
                            licenseTemplate.LicenseType = ContentKeyPolicyPlayReadyLicenseType.NonPersistent;
                            break;
                        case "Persistent":
                            licenseTemplate.LicenseType = ContentKeyPolicyPlayReadyLicenseType.Persistent;
                            break;
                        default:
                            return new BadRequestObjectResult("Please pass valid licenseType in the input object");
                    }
                }
                if (data.contentKeyLocation != null)
                {
                    switch (data.contentKeyLocation)
                    {
                        case "ContentEncryptionKeyFromHeader":
                            licenseTemplate.ContentKeyLocation = new ContentKeyPolicyPlayReadyContentEncryptionKeyFromHeader();
                            break;
                        case "ContentEncryptionKeyFromKeyIdentifier":
                            licenseTemplate.ContentKeyLocation = new ContentKeyPolicyPlayReadyContentEncryptionKeyFromKeyIdentifier();
                            break;
                        default:
                            return new BadRequestObjectResult("Please pass valid contentKeyLocation in the input object");
                    }
                }
                if (data.contentType != null)
                {
                    switch (data.contentType)
                    {
                        case "Unspecified":
                            licenseTemplate.ContentType = ContentKeyPolicyPlayReadyContentType.Unspecified;
                            break;
                        case "UltraVioletDownload":
                            licenseTemplate.ContentType = ContentKeyPolicyPlayReadyContentType.UltraVioletDownload;
                            break;
                        case "UltraVioletStreaming":
                            licenseTemplate.ContentType = ContentKeyPolicyPlayReadyContentType.UltraVioletStreaming;
                            break;
                        default:
                            return new BadRequestObjectResult("Please pass valid contentType in the input object");
                    }
                }

                if (data.beginDate != null) licenseTemplate.BeginDate = data.beginDate;
                if (data.expirationDate != null) licenseTemplate.ExpirationDate = data.expirationDate;
                if (data.relativeBeginDate != null) licenseTemplate.RelativeBeginDate = System.Xml.XmlConvert.ToTimeSpan(data.relativeBeginDate);
                if (data.relativeExpirationDate != null) licenseTemplate.RelativeExpirationDate = System.Xml.XmlConvert.ToTimeSpan(data.relativeExpirationDate);
                if (data.gracePeriod != null) licenseTemplate.GracePeriod = System.Xml.XmlConvert.ToTimeSpan(data.gracePeriod);

                if (data.digitalVideoOnlyContentRestriction != null) licenseTemplate.PlayRight.DigitalVideoOnlyContentRestriction = data.digitalVideoOnlyContentRestriction;
                if (data.imageConstraintForAnalogComponentVideoRestriction != null) licenseTemplate.PlayRight.ImageConstraintForAnalogComponentVideoRestriction = data.imageConstraintForAnalogComponentVideoRestriction;
                if (data.imageConstraintForAnalogComputerMonitorRestriction != null) licenseTemplate.PlayRight.ImageConstraintForAnalogComputerMonitorRestriction = data.imageConstraintForAnalogComputerMonitorRestriction;
                if (data.allowPassingVideoContentToUnknownOutput != null)
                {
                    switch (data.allowPassingVideoContentToUnknownOutput)
                    {
                        case "NotAllowed":
                            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.NotAllowed;
                            break;
                        case "Allowed":
                            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.Allowed;
                            break;
                        case "AllowedWithVideoConstriction":
                            licenseTemplate.PlayRight.AllowPassingVideoContentToUnknownOutput = ContentKeyPolicyPlayReadyUnknownOutputPassingOption.AllowedWithVideoConstriction;
                            break;
                        default:
                            return new BadRequestObjectResult("Please pass valid allowPassingVideoContentToUnknownOutput in the input object");
                    }
                }
                if (data.firstPlayExpiration != null) licenseTemplate.PlayRight.FirstPlayExpiration = System.Xml.XmlConvert.ToTimeSpan(data.firstPlayExpiration);
                if (data.scmsRestriction != null) licenseTemplate.PlayRight.ScmsRestriction = data.scmsRestriction;
                if (data.agcAndColorStripeRestriction != null) licenseTemplate.PlayRight.AgcAndColorStripeRestriction = data.agcAndColorStripeRestriction;
                if (data.explicitAnalogTelevisionOutputRestriction != null || data.explicitAnalogTelevisionOutputRestrictionBestEffort != null)
                {
                    licenseTemplate.PlayRight.ExplicitAnalogTelevisionOutputRestriction = new ContentKeyPolicyPlayReadyExplicitAnalogTelevisionRestriction();
                    if (data.explicitAnalogTelevisionOutputRestriction != null)
                        licenseTemplate.PlayRight.ExplicitAnalogTelevisionOutputRestriction.ConfigurationData = data.explicitAnalogTelevisionOutputRestriction;
                    if (data.explicitAnalogTelevisionOutputRestrictionBestEffort != null)
                        licenseTemplate.PlayRight.ExplicitAnalogTelevisionOutputRestriction.BestEffort = data.explicitAnalogTelevisionOutputRestrictionBestEffort;
                }
                if (data.uncompressedDigitalVideoOpl != null) licenseTemplate.PlayRight.UncompressedDigitalVideoOpl = data.uncompressedDigitalVideoOpl;
                if (data.compressedDigitalVideoOpl != null) licenseTemplate.PlayRight.CompressedDigitalVideoOpl = data.compressedDigitalVideoOpl;
                if (data.analogVideoOpl != null) licenseTemplate.PlayRight.AnalogVideoOpl = data.analogVideoOpl;
                if (data.uncompressedDigitalAudioOpl != null) licenseTemplate.PlayRight.UncompressedDigitalAudioOpl = data.uncompressedDigitalAudioOpl;
                if (data.compressedDigitalAudioOpl != null) licenseTemplate.PlayRight.CompressedDigitalAudioOpl = data.compressedDigitalAudioOpl;

                licenseTemplate.Validate();
                licenseTemplates.Add(licenseTemplate);

                string jsonString = JsonConvert.SerializeObject(licenseTemplates, jsonWriter);
                jToken = JToken.Parse(jsonString);

                //var settings = new JsonSerializerSettings();
                //settings.TypeNameHandling = TypeNameHandling.Auto;
                //string jsonString = JsonConvert.SerializeObject(licenseTemplates, settings);
                //jToken = MediaServicesHelper.ConvertTypeInMediaServicesJson(jsonString);

                //jToken = MediaServicesHelper.ConvertTypeInMediaServicesJson(licenseTemplates);

            }
            catch (ApiErrorException e)
            {
                log.LogInformation($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.LogInformation($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                playReadyLicenses = jToken
            });
        }
    }
}
