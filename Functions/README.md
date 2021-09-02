---
services: media-services,functions,logic-app
platforms: dotnetcore
author: xpouyat
---

# .NET 5 Functions for Azure Media Services v3

This project contains examples of Azure Functions for Azure Media Services v3.
You can use Visual Studio 2019 or Visual Studio Code to run and deploy the functions.

## .NET solution file and how to launch project

Open the root /Functions/Functions.sln (or just open the Functions folder in VS Code).
The main solution contains the Azure Functions project.

When using VS Code, you can launch the Functions in the Debugger console (Ctrl-shift-D).

This Media Services Functions example is based on .NET 5 Azure Functions.

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

### 4. Run the Functions locally

With Visual Studio or [Visual Studio Code](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#run-the-function-locally).

### 5. Deploy the Azure functions to Azure

VSCode or Azure CLI is recommended for the deployment.
For Aure CLI commands, read [this](https://github.com/Azure/azure-functions-dotnet-worker).

With VSCode, select the Azure tab and click on the icon to deploy it to Azure.

![Screen capture](../Images/azfunc5deploy.png?raw=true)

To get the function Url, use the Functions explorer in VSCode.

![Screen capture](../Images/azfunc5geturl.png?raw=true)

More details in the [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#sign-in-to-azure).

## Functions documentation

### SubmitEncodingJob Funtion

Input body :

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