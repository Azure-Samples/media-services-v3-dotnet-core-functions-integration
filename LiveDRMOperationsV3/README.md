---
services: media-services,functions
platforms: dotnetcore
author: xpouyat
---

# Media Services v3 Dynamic Encryption with Irdeto license delivery service
This Visual Studio 2017 Solution exposes several Azure functions that can be used to manage live streaming with DRM, using Irdeto back-end to deliver the licenses. The functions communicate with Irdeto backend using SOAP. Optionaly, a Cosmos database can be used to store the result of the functions, and to specify the settings of the live event(s) to be created :ArchiveWindow, baseStorageName, ACLs, autostart, Vanity URL mode.

Here are the list of functions:

- check-all-live-event-output
- create-clear-streaming-locator
- create-live-event-output
- delete-live-event-output
- delete-streaming-locator
- reset-live-event-output
- start-live-event
- update-settings

As an option, two AMS accounts ad two Azure functions deployment can be created in two different datacenters. An Azure function deployment could manage either AMS account. Fot this, it is needed that the AMS account names ended with 2 or 4 letters which defines the region (euwe/euno OR we/no). Resource group names could have the same convention name or a single resource group name can be used (in that case, use ResourceGroupFinalName proporty set to a non empty string).

This Media Services Functions code is based on AMS REST API v3 on Azure Functions v2.

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
    "AzureRegion" : "euwe", // optional entry. Default extension to be added to AMS account name, baseStorageName in settings, and Resource Group
    "ResourceGroup": "value",
    "ResourceGroupFinalName" : "true", // optional entry. Tells the code to not append the AzureRegion code the Resource Group. Put any non empty string
    "SubscriptionId": "value",
    "IrdetoUserName": "value",
    "IrdetoPassword": "value",
    "IrdetoAccountId": "value",
    "IrdetoSoapService": "value",
    "IrdetoPlayReadyLAURL": "value",
    "IrdetoWidevineLAURL": "value",
    "IrdetoFairPlayLAURL": "value",
    "CosmosDBAccountEndpoint": "value", /* optional but needed for Cosmos support */
    "CosmosDBAccountKey": "value", // optional but needed for Cosmos support
    "CosmosDB": "liveDRMStreaming", // optional but needed for Cosmos support
    "CosmosCollectionSettings": "liveEventSettings", // optional but needed for Settings
    "CosmosCollectionOutputs": "liveEventOutputInfo", // optional but needed for storing the output to Cosmos
    "PreferredStreamingEndpoint" : "" // optional, use only by redirector
  }
}
```