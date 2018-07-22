//
// Azure Media Services REST API v3 Functions
//
// MonitorMediaJob - This function monitors media job.
//
/*
```c#
Input:
    {
        // Name of the media job
        "jobName": "amsv3function-job-24369d2e-7415-4ff5-ba12-b8a879a15401",
        // Name of the Transform for the media job
        "transformName": "TestTransform"
    }
Output:
    {
        // Status name of the media job
        "jobStatus": "Finished",
        // Status of each task/output asset in the media job
        "jobOutputStateList": [
            {
                // Name of the Output Asset
                "AssetName": "out-testasset-efbf71e8-3f80-480d-9b92-f02bef6ad4d2",
                // Status of the media task for the Output Asset
                "State": "Finished",
                // Progress of the media task for the Output Asset
                "Progress": 100
            },
            ...
        ]
    }

```
*/
//      // https://docs.microsoft.com/en-us/dotnet/api/microsoft.windowsazure.mediaservices.client.jobstate?view=azure-dotnet
//      //      Queued      0
//      //      Scheduled   1
//      //      Processing  2
//      //      Finished    3
//      //      Error       4
//      //      Canceled    5
//      //      Canceling   6
//
//

using System;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class MonitorMediaJob
    {
        [FunctionName("MonitorMediaJob")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - MonitorMediaJob was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            // Validate input objects
            if (data.jobName == null)
                return new BadRequestObjectResult("Please pass jobName in the input object");
            if (data.transformName == null)
                return new BadRequestObjectResult("Please pass transformName in the input object");
            string jobName = data.jobName;
            string transformName = data.transformName;


            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            Job job = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);
                job = client.Jobs.Get(amsconfig.ResourceGroup, amsconfig.AccountName, transformName, jobName);

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

            JObject result = new JObject();
            result["jobStatus"] = job.State.ToString();
            JArray jobOutputStateList = new JArray();
            foreach (JobOutputAsset o in job.Outputs)
            {
                JObject jobOutputState = new JObject();
                jobOutputState["AssetName"] = o.AssetName;
                jobOutputState["State"] = o.State.ToString();
                jobOutputState["Progress"] = o.Progress;
                if (o.Error != null)
                {
                    jobOutputState["ErrorCode"] = o.Error.Code.ToString();
                    jobOutputState["ErrorMessage"] = o.Error.Message.ToString();
                }
                jobOutputStateList.Add(jobOutputState);
            }
            result["jobOutputStateList"] = jobOutputStateList;

            return (ActionResult)new OkObjectResult(result);
        }
    }
}
