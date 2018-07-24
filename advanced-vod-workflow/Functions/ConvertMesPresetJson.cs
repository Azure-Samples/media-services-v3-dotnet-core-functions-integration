//
// Azure Media Services REST API v3 Functions
//
// CreateTransform - This function creates a new transform.
//
/*
```c#
Input:
    {
    }
Output:
    {
    }

```
*/
//
//

using System;
using System.IO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3.Functions
{
    public static class ConvertMesPresetJson
    {
        [FunctionName("ConvertMesPresetJson")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - ConvertMesPresetJson was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.mesPresetJson == null)
                return new BadRequestObjectResult("Please pass mesPresetJson in the input object");
            string mesPresetJson = data.mesPresetJson;

            //MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string presetString = null;

            try
            {
                //IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);
                JsonConverter[] v2JsonConverters = { new MediaServicesHelperJsonReader() };
                MESPresetSchema v2preset = JsonConvert.DeserializeObject<MESPresetSchema>(mesPresetJson, v2JsonConverters);
                StandardEncoderPreset v3preset = MediaServicesHelperMES.convertMESPreset(v2preset);
                JsonConverter[] v3JsonConverters = { new MediaServicesHelperJsonWriter(), new MediaServicesHelperTimeSpanJsonConverter() };
                presetString = JsonConvert.SerializeObject(v3preset, v3JsonConverters);

            }
            catch (ApiErrorException e)
            {
                log.Info($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.Info($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                preset = presetString
            });
        }
    }
}
