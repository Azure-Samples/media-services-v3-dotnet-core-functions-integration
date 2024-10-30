---
page_type: sample
languages:
- csharp
products:
- azure
- azure-functions
- azure-media-services
name: "Azure Media Services v3 - Serverless Workflows with Azure Functions and Logic Apps"
description: "Projects that show how to integrate Azure Media Services with Azure Functions and Azure Logic Apps."
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/media-services-v3-dotnet-core-functions-integration/master/azuredeploy.json
---

# Azure Media Services v3 - Serverless Workflows with Azure Functions & Logic Apps

**IMPORTANT NOTE : Azure Media Services have been retired on June 30, 2024. Please see the [retirement guide](https://learn.microsoft.com/en-us/azure/media-services/latest/azure-media-services-retirement).**

This repository contains projects that show how to integrate Azure Media Services with Azure Functions & Azure Logic Apps.
These Media Services Functions examples are based on AMS REST API v3 on Azure Functions v3. Most of the functions can also be used from Logic Apps.

This repository can be accessed directly using <https://aka.ms/ams3functions>.

## Contents

| Folder | Description |
|-------------|-------------|
| [Functions](/Functions)|**Updated March 2023** This sample exposes Azure Functions based on .NET 7.0 using the latest Media Services, Azure Functions and Identity SDKs. Deployment is done with an ARM template and GitHub Actions.|
| [Encoding](/Encoding)|The sample exposes an Azure Function that encodes an Azure Storage blob with ffmpeg. Azure Functions Premium plan is recommended.|
| [LiveAndVodDRMOperationsV3](/LiveAndVodDRMOperationsV3)|The sample exposes several Azure functions that can be used to manage live streaming and VOD with DRM with Azure Media Services v3, using Irdeto back-end to deliver the licenses.|
| [advanced-vod-workflow](/advanced-vod-workflow)|This project contains advanced VOD media workflow examples of using Azure Functions with Azure Media Services v3. The project includes several folders of sample Azure Functions for use with Azure Media Services that show workflows related to ingesting content directly from blob storage, encoding, and writing content back to blob storage.|
| [logic-app-using-workflow-functions](/logic-app-using-workflow-functions)|This project contains Logic Apps that converts-to-media-asset, encodes and publishes media files that you upload in an Azure Storage. It relies on Azure Functions and Azure Media Services.|

## Prerequisites for a sample Logic Apps deployments

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/create-account-howto?tabs=portal)).

### 2. Create a Service Principal

Create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/access-api-howto?tabs=portal)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint.

### 4. Deploy the Azure functions

For the 'Functions" project, do not use the link below. Please see the dedicated [Readme](/Functions/README.md).

If not already done : fork the repo, deploy Azure Functions and select the right project (IMPORTANT!).

Note : if you never provided your GitHub account in the Azure portal before, the continuous integration probably will probably fail and you won't see the functions. In that case, you need to setup it manually. Go to your Azure Function App / Deployment / Deployment Center. Select GitHub as a source and configure it to use your fork.

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmaster%2Fazuredeploy.json)
