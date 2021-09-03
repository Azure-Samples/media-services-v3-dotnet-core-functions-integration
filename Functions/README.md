---
services: media-services,functions,logic-app
platforms: dotnetcore
author: xpouyat
---

# .NET 5 Functions for Azure Media Services v3

This project contains examples of Azure Functions that connect to Azure Media Services v3 for video processing.
You can use Visual Studio 2019 or Visual Studio Code to run and deploy the functions.

The **SubmitEncodingJob** function takes a Media Services asset or a source URL and launches an encoding job with Media Services. It uses a Transform which is created if it does not exist. When it is created, it used the preset provided in the input body. More information at the end this readme file.

## .NET solution file and how to launch project

Open the root /Functions/Functions.sln (or just open the Functions folder in VS Code).
The main solution contains the Azure Functions project.

When using VS Code, you can launch the Functions in the Debugger console (Ctrl-shift-D).

For more on information on .NET 5 & Azure Functions, see [this repository](https://github.com/Azure/azure-functions-dotnet-worker).

## How to set variables for a local execution

Create a **.env file** at the root with your account settings.
For more information, see [Access APIs](https://docs.microsoft.com/en-us/azure/media-services/latest/access-api-howto).

Use [sample.env](../sample.env) as a template for the .env file to be created. The .env file must be placed at the root of the sample (same location than sample.env).
Connect to the Azure portal with your browser and go to your media services account / API access to get the .ENV data to store to the .env file.

Then build and run the sample in Visual Studio or VS Code.

## Prerequisites

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/create-account-howto?tabs=portal)).

### 2. Create a Service Principal

Create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/access-api-howto?tabs=portal)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint.

## How to run and deploy

### 1. Run the Functions locally

Make sure that you have a .env file created and filled correctly.

Then use Visual Studio or [Visual Studio Code](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#run-the-function-locally) to run the functions.

Use Postman to test your local functions.

![Screen capture](../Images/azfunc5deploy.png?raw=true)

### 2. Deploy the Azure functions to Azure

VS Code or Azure CLI is recommended for the deployment.
For Aure CLI commands, read [this](https://github.com/Azure/azure-functions-dotnet-worker).

With VS Code, select the Azure tab and click on the icon to deploy it to Azure.

![Screen capture](../Images/azfunc5deploy.png?raw=true)

To get the function Url, use the Functions explorer in VSCode, or the Azure portal.

![Screen capture](../Images/azfunc5geturl.png?raw=true)

More details in the [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#sign-in-to-azure).

Note : we recommend deploying the functions using VS Code as it has been successfully tested. Deploying them using the ARM template [azuredeploy2.json](../azuredeploy2.json) and continuous integration from GitHub seems problematic for now (functions are not visible in the deployment).

### Add your settings to the deployment

Go the Azure portal, select your Azure functions deployment, go to the 'Configuration' tab, select 'Advanced Edit' and add the following entries with your values.

```json
  {
    "name": "AADCLIENTID",
    "value": "00000000-0000-0000-0000-000000000000",
    "slotSetting": false
  },
  {
    "name": "AADSECRET",
    "value": "00000000-0000-0000-0000-000000000000",
    "slotSetting": false
  },
  {
    "name": "AADTENANTDOMAIN",
    "value": "microsoft.onmicrosoft.com",
    "slotSetting": false
  },
  {
    "name": "AADTENANTID",
    "value": "00000000-0000-0000-0000-000000000000",
    "slotSetting": false
  },
  {
    "name": "ACCOUNTNAME",
    "value": "amsaccount",
    "slotSetting": false
  },
  {
    "name": "RESOURCEGROUP",
    "value": "amsResourceGroup",
    "slotSetting": false
  },
  {
    "name": "ARMAADAUDIENCE",
    "value": "https://management.core.windows.net",
    "slotSetting": false
  },
  {
    "name": "ARMENDPOINT",
    "value": "https://management.azure.com",
    "slotSetting": false
  },
  {
    "name": "SUBSCRIPTIONID",
    "value": "00000000-0000-0000-0000-000000000000",
    "slotSetting": false
  }
```

These application settings are used by the functions to connect to your Media Services account.

![Screen capture](../Images/azfunc5deployappsettings.png?raw=true)

Use Postman to test your Azure functions.

## Functions documentation

### **SubmitEncodingJob** Function

This function processes a Media Services asset or a source URL. It launches a job using the Transform name provided in the input. If the Transform does not exist, the function creates based on the provided Media Encoder Standard preset. The function returns back the output asset name and job name.

See the model for the [input](Functions/SubmitEncodingJob.cs#L22-L63) and [output](Functions/SubmitEncodingJob.cs#L65-L81).

Input body sample :

```json
{
    "inputUrl":"https://nimbuscdn-nimbuspm.streaming.mediaservices.windows.net/2b533311-b215-4409-80af-529c3e853622/Ignite-short.mp4",
    "transformName" : "TransformAS",
    "builtInPreset" :"AdaptiveStreaming"
 }
 ```

Or

```json
{
    "inputAssetName":"input-dgs4fss5",
    "transformName" : "TransformAS",
    "builtInPreset" :"AdaptiveStreaming"
 }
 ```
