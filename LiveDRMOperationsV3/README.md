---
services: media-services,functions
platforms: dotnetcore
author: xpouyat
---

# Media Services v3 Dynamic Encryption with Irdeto license delivery service
This Visual Studio 2017 Solution exposes several Azure functions that can be used to protect live or on-demand assets with DRM, using Irdeto back-end to deliver the licenses. The functions dialog with Irdeto using SOAP. There is a function to create a streaming policy, a function to setup DRM and create a locator on an asset, and a function to delete a locator.

This Media Services Functions example is based on AMS REST API v3 on Azure Functions v2.

## Prerequisites for a sample Logic Apps deployments

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-create-account)).

### 2. Create a Service Principal

Create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-get-started-with-aad#service-principal-authentication)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-vod-get-started#start-the-streaming-endpoint)).

### 4. Deploy the Azure functions
If not already done : fork the repo, download a local copy. Open the solution with Visual Studio and publish the functions to Azure.

These functions have been designed to work with Irdeto back-end. It requires credentials and urls to be set in application settings. If you run the functions locally, you need to specify the values in the local settings file.

local.settings.json will look like (please replace 'value' with the correct data):

```json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "AadClientId": "value",
    "AadEndpoint": "https://login.microsoftonline.com",
    "AadSecret": "value",
    "AadTenantId": "value",
    "AccountName": "value",
    "ArmAadAudience": "https://management.core.windows.net/",
    "ArmEndpoint": "https://management.azure.com/",
    "Region": "value",
    "ResourceGroup": "value",
    "SubscriptionId": "value",
    "IrdetoUserName": "value",
    "IrdetoPassword": "value",
    "IrdetoAccountId": "value",
    "IrdetoSoapService": "value",
    "IrdetoPlayReadyLAURL": "value",
    "IrdetoWidevineLAURL": "value",
    "IrdetoFairPlayLAURL": "value",
    "CosmosDBAccountEndpoint": "value",
    "CosmosDBAccountKey": "value",
    "CosmosDB": "liveDRMStreaming",
    "CosmosCollectionSettings": "liveEventSettings",
    "CosmosCollectionOutputs": "liveEventOutputInfo"
  }
}
```