---
services: media-services,functions
platforms: dotnetcore
author: xpouyat
---

# Ffmpeg encoding with an Azure function
This Visual Studio 2017 Solution exposes an Azure Function that encodes an Azure Storage blob with ffmpeg. [Azure Functions Premium plan](https://docs.microsoft.com/en-us/azure/azure-functions/functions-premium-plan
) is recommended.

This Functions code is based on Azure Functions v2.

### Fork and download a copy
If not already done : fork the repo, download a local copy.

### Ffmpeg
Download ffmpeg from the Internet and copy ffmpeg.exe in \Encoding\Encoding\ffmpeg folder.
In Visual Studio, open the file properties, "Build action" should be "Content", with "Copy to Output Direcotry" set to "Copy if newer".

### Publish the function to Azure
Open the solution with Visual Studio and publish the functions to Azure.
It is recommended to use a **premium plan** to avoid functions timeout (Premium gives you 30 min and a more powerfull host).
It is possible to [unbound run duration](https://docs.microsoft.com/en-us/azure/azure-functions/functions-premium-plan#longer-run-duration).

JSON input body of the function :
```json
{
    "sasInputUrl":"https://mysasurlofthesourceblob",
    "sasOutputUrl":"https://mysasurlofthedestinationblob",
    "ffmpegArguments" : " -i {input} {output} -y"
}
```
