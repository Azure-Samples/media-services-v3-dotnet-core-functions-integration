//
// Azure Media Services REST API v3 Functions
//
// list-assets-startwith
/*
This function creates an empty asset.


```c#
Input:
{
    "assetNameStartsWith" : "movie-", // 
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, the live event will be deleted from the two regions.
            // Note: the service principal must work with all this accounts
}


Output:
{
    "success": true,
    "assetNames": [
        "92ccbf1edb-input-59894",
        "92ccbf1edb-input-59894-ContentAwareEncode-output-59894"
    ],
    "operationsVersion": "1.0.1.0"
}

```
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using LiveDRMOperationsV3.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure.OData;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LiveDrmOperationsV3
{
    public static class ListAssetsStartwith
    {
        [FunctionName("list-assets-startwith")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log, Microsoft.Azure.WebJobs.ExecutionContext execContext)
        {
            MediaServicesHelpers.LogInformation(log, "C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            var assetNameStartsWith = (string)data.assetNameStartsWith;
            if (assetNameStartsWith == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass assetNameStartsWith in the JSON");

            List<string> assets = new List<string>();

            // Azure region management
            var azureRegions = new List<string>();
            if ((string)data.azureRegion != null)
            {
                azureRegions = ((string)data.azureRegion).Split(',').ToList();
            }
            else
            {
                azureRegions.Add((string)null);
            }


            foreach (var region in azureRegions)
            {
                ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddEnvironmentVariables()
                        .Build(),
                        region
                );

                MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                // Set the polling interval for long running operations to 2 seconds.
                // The default value is 30 seconds for the .NET client SDK
                client.LongRunningOperationRetryTimeout = 2;

                MediaServicesHelpers.LogInformation(log, "asset name starts : " + assetNameStartsWith, region);

                try
                {
                    ODataQuery<Asset> query = new ODataQuery<Asset>();
                    string search = "'" + assetNameStartsWith + "'";
                    query.Filter = "name gt " + search.Substring(0, search.Length - 2) + char.ConvertFromUtf32(char.ConvertToUtf32(search, search.Length - 2) - 1) + new string('z', 262 - search.Length) + "'" + " and name lt " + search.Substring(0, search.Length - 1) + new string('z', 262 - search.Length) + "'";
                    var assetsResult = client.Assets.List(config.ResourceGroup, config.AccountName, query);

                    assets = assetsResult.Select(a => a.Name).ToList();
                }
                catch (Exception ex)
                {
                    return IrdetoHelpers.ReturnErrorException(log, ex);
                }
            }

            var response = new JObject
            {
                {"success", true},
                {"assetNames", new JArray(assets)},
                {
                    "operationsVersion",
                    AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                }
            };

            return new OkObjectResult(
                response
            );
        }
    }
}