//
// 
//
// ffmpeg -  This function encode with ffmpeg. Recommended is to deploy using the Premium plan
//
/*
```c#
Input :
{
    "sasInputUrl":"",
    "sasOutputUrl":"",
    "ffmpegArguments" : " -i {input} {output} -y"
}


```
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Encoding.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Encoding
{
    public static class fmpeg
    {

        [FunctionName("ffmpeg-encoding")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log, ExecutionContext context)
        {
            string output = string.Empty;
            bool isSuccessful = true;
            dynamic ffmpegResult = new JObject();
            string errorText = string.Empty;
            int exitCode = 0;

            log.LogInformation("C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return Helpers.Helpers.ReturnErrorException(log, ex);
            }

            var ffmpegArguments = (string)data.ffmpegArguments;

            var sasInputUrl = (string)data.sasInputUrl;
            if (sasInputUrl == null)
                return Helpers.Helpers.ReturnErrorException(log, "Error - please pass sasInputUrl in the JSON");

            var sasOutputUrl = (string)data.sasOutputUrl;
            if (sasOutputUrl == null)
                return Helpers.Helpers.ReturnErrorException(log, "Error - please pass sasOutputUrl in the JSON");


            log.LogInformation("Arguments : ");
            log.LogInformation(ffmpegArguments);

            try
            {
                var folder = context.FunctionDirectory;

                string inputFileName = System.IO.Path.GetFileName(new Uri(sasInputUrl).LocalPath);
                string pathLocalInput = System.IO.Path.Combine(context.FunctionDirectory, inputFileName);

                string outputFileName = System.IO.Path.GetFileName(new Uri(sasOutputUrl).LocalPath);
                string pathLocalOutput = System.IO.Path.Combine(context.FunctionDirectory, outputFileName);

                foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    log.LogInformation($"{drive.Name}: {drive.TotalFreeSpace / 1024 / 1024} MB");
                }

                /* Downloads the original video file from blob to local storage. */
                log.LogInformation("Dowloading source file from blob to local");
                using (FileStream fs = System.IO.File.Create(pathLocalInput))
                {
                    try
                    {
                        var readBlob = new CloudBlob(new Uri(sasInputUrl));
                        await readBlob.DownloadToStreamAsync(fs);
                        log.LogInformation("Downloaded input file from blob");
                    }
                    catch (Exception ex)
                    {
                        log.LogError("There was a problem downloading input file from blob. " + ex.ToString());
                    }
                }

                log.LogInformation("Encoding...");
                var file = System.IO.Path.Combine(folder, "..\\ffmpeg\\ffmpeg.exe");

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = file;

                process.StartInfo.Arguments = (ffmpegArguments ?? " -i {input} {output} -y")
                    .Replace("{input}", "\"" + pathLocalInput + "\"")
                    .Replace("{output}", "\"" + pathLocalOutput + "\"");


                log.LogInformation(process.StartInfo.Arguments);

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += new DataReceivedEventHandler(
                    (s, e) =>
                    {
                        log.LogInformation("O: " + e.Data);
                    }
                );
                process.ErrorDataReceived += new DataReceivedEventHandler(
                    (s, e) =>
                    {
                        log.LogInformation("E: " + e.Data);
                    }
                );
                //start process
                process.Start();
                log.LogInformation("process started");
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                exitCode = process.ExitCode;
                ffmpegResult = output;

                log.LogInformation("Video Converted");

                /* Uploads the encoded video file from local to blob. */
                log.LogInformation("Uploading encoded file to blob");
                using (FileStream fs = System.IO.File.OpenRead(pathLocalOutput))
                {
                    try
                    {
                        var writeBlob = new CloudBlockBlob(new Uri(sasOutputUrl));
                        await writeBlob.UploadFromStreamAsync(fs);
                        log.LogInformation("Uploaded encoded file to blob");
                    }
                    catch (Exception ex)
                    {
                        log.LogInformation("There was a problem uploading converted file to blob. " + ex.ToString());
                    }
                }
                System.IO.File.Delete(pathLocalInput);
                System.IO.File.Delete(pathLocalOutput);
            }
            catch (Exception e)
            {
                isSuccessful = false;
                errorText += e.Message;
            }

            if (exitCode != 0)
            {
                isSuccessful = false;
            }

            var response = new JObject
            {
                {"isSuccessful", isSuccessful},
                {"ffmpegResult",  ffmpegResult},
                {"errorText", errorText }

            };

            return new OkObjectResult(
                response
            );
        }
    }
}