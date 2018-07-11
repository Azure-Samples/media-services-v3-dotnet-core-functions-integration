---
services: media-services,functions,logic-app
platforms: dotnetcore
author: shigeyf
---

# Media Services: Integrating Azure Media Services with Azure Functions & Azure Logic Apps
This project contains advanced VOD media workflow examples of using Azure Functions with Azure Media Services. 
The project includes several folders of sample Azure Functions for use with Azure Media Services that show workflows related to ingesting content directly from blob storage, encoding, and writing content back to blob storage.

This Media Services Functions example is based on AMS REST API v3 on Azure Functions v2.


## Prerequisites for a sample Logic Apps deployments

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-create-account)).

### 2. Create a Service Principal

Create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-get-started-with-aad#service-principal-authentication)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-vod-get-started#start-the-streaming-endpoint)).

### 4. Deploy the Azure functions
If not already done : fork the repo, deploy Azure Functions and select the **"advanced-vod-workflow-functions"** Project (IMPORTANT!)

Follow the guidelines in the [git tutorial](1-CONTRIBUTION-GUIDE/git-tutorial.md) for details on how to fork the project and use Git properly with this project.

Note : if you never provided your GitHub account in the Azure portal before, the continous integration probably will probably fail and you won't see the functions. In that case, you need to setup it manually. Go to your azure functions deployment / Functions app settings / Configure continous integration. Select GitHub as a source and configure it to use your fork.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fshigeyf%2Fmedia-services-dotnet-functions-integration%2Fmaster%2Fadvanced-vod-workflow-functions-v3%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>



## Functions documentation
This [page](Functions-documentation.md) lists the functions available and describes the input and output parameters.