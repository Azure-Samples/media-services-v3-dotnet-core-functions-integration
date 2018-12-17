---
services: media-services,functions
platforms: dotnetcore
author: xpouyat
---

# Media Services v3 Dynamic Encryption with Irdeto license delivery service
This Visual Studio 2017 Solution exposes several Azure functions that can be used to manage live streaming with DRM, using Irdeto back-end to deliver the licenses. The functions communicate with Irdeto backend using SOAP. Optionaly, a Cosmos database can be used to store the result of the functions, and to specify the settings of the live event(s) to be created : ArchiveWindow, baseStorageName, ACLs, autostart, Vanity URL mode.

Here are the list of functions:

- check-all-live-event-output
- create-clear-streaming-locator
- create-live-event-output
- delete-live-event-output
- delete-streaming-locator
- ping
- redirector
- reset-live-event-output
- start-live-event
- stop-live-event
- update-settings

As an option, two AMS accounts and two Azure functions deployments can be created in two different Azure regions. An Azure function deployment could manage either AMS account. For this to work, it is needed that the AMS account names ended with 2 or 4 letters which defines the region (euwe/euno OR we/no). Resource group names could have the same convention name, or a single resource group name can be used (in that case, use ResourceGroupFinalName proporty set to a non empty string).

It is also possible to execute all operations in two regions at the same time. For example, the creation of a live event can be executed by the function into two different accounts simultaneously. In that case, the same live event name, ingest path, output locator name, output locator GUID, streaming policy name and DRM keys will be used ; all URLS will be similar (except the hostname, of course). To use this mode, pass the two regions to the create-live-event-output function. Example :
```json
{
  "liveEventName": "Channel1",
  "azureRegion" : "euwe,euno"
}
```

This Media Services Functions code is based on AMS REST API v3 on Azure Functions v2.

Overall architecture :

![Screen capture](images/overview.png?raw=true)

## Prerequisites for a sample Logic Apps deployments

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-create-account)).

### 2. Create a Service Principal

Create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/media-services-portal-get-started-with-aad#service-principal-authentication)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-portal-vod-get-started#start-the-streaming-endpoint)).

### 4. Deploy the Azure functions
If not already done : fork the repo, download a local copy. Open the solution with Visual Studio and publish the functions to Azure.
It is recommended to use a **dedicated plan** to avoid functions timeout.
The redirector function (with anonymous access), if used, should be deployed on a separate plan as a **consumption plan** to get better scalability if this function is called by end-users.


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
    "AllowClearStream" : "true" // optional, use only by redirector
  }
}
```

### 5. Optional : deploy a Cosmos Database
This database is used to read the settings when creating a live event. It is also used to store all the information about the live event and output created.

Example of settings in Cosmos for a live event :
```json
{
    "liveEventName": "TESTLIVEEVENT",
    "urn": "urn:customer:video:e8927fcf-e1a0-0001-7edd-1eaaaaaa",
    "vendor": "Customer",
    "baseStorageName": "uhddstvstreaming",
    "archiveWindowLength": 10,
    "vanityUrl": true,
    "lowLatency": false,
    "liveEventInputACL": [
        "192.168.0.0/24",
        "86.246.149.14"
    ],
    "liveEventPreviewACL": [
        "192.168.0.0/24",
        "86.246.149.14"
    ],
    "playerJSONData": null,
    "redirectorStreamingEndpointData":[
      {"StreamingEndpointName":"verizon", "Percentage":"50"},
      {"StreamingEndpointName":"akamai", "Percentage":"50"}
    ],
    "id": "TESTLIVEEVENT"
}
```

Example of information in Cosmos for a live event :
```json
{
      "liveEventName": "CH1",
      "resourceState": "Running",
      "vanityUrl": true,
      "amsAccountName": "customerssrlivedeveuwe",
      "region": "West Europe",
      "resourceGroup": "GD-INIT-DISTLSV-dev-euwe",
      "lowLatency": false,
      "id": "customerssrlivedeveuwe:CH1",
      "input": [
        {
          "protocol": "FragmentedMP4",
          "url": "http://CH1-customerssrlivedeveuwe-euwe.channel.media.azure.net/838afbbac2514fafa2eaed76d8a3cc74/ingest.isml"
        }
      ],
      "inputACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "preview": [
        {
          "protocol": "FragmentedMP4",
          "url": "https://CH1-customerssrlivedeveuwe.preview-euwe.channel.media.azure.net/90083bd1-bed3-4019-9d54-b70e314ac9c8/preview.ism/manifest"
        }
      ],
      "previewACL": [
        "192.168.0.0/24",
        "86.246.149.14/0"
      ],
      "liveOutputs": [
        {
          "liveOutputName": "output-179744a9-3f6f",
          "archiveWindowLength": 120,
          "assetName": "asset-179744a9-3f6f",
          "assetStorageAccountName": "rsilsvdeveuwe",
          "resourceState": "Running",
          "streamingLocators": [
            {
              "streamingLocatorName": "locator-179744a9-3f6f",
              "streamingPolicyName": "CH1-321870db-de01",
              "cencKeyId": "58420ba1-da30-4756-b50c-fcd72a9645b7",
              "cbcsKeyId": "ced687fd-c34b-433e-bca7-346a1d7af9f5",
              "drm": [
                {
                  "type": "FairPlay",
                  "licenseUrl": "skd://rng.live.ott.irdeto.com/licenseServer/streaming/v1/CUSTOMER/getckc?ContentId=CH1&KeyId=ced687fd-c34b-433e-bca7-346a1d7af9f5",
                  "protocols": [
                    "DashCmaf",
                    "HlsCmaf",
                    "HlsTs"
                  ]
                },
                {
                  "type": "PlayReady",
                  "licenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/playready/v1/CUSTOMER/license?ContentId=CH1",
                  "protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
                },
                {
                  "type": "Widevine",
                  "licenseUrl": "https://rng.live.ott.irdeto.com/licenseServer/widevine/v1/CUSTOMER/license&ContentId=CH1",
                  "protocols": [
                    "DashCmaf",
                    "DashCsf"
                  ]
                }
              ],
              "urls": [
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(encryption=cenc)",
                  "protocol": "SmoothStreaming"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-csf,encryption=cenc)",
                  "protocol": "DashCsf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=mpd-time-cmaf,encryption=cenc)",
                  "protocol": "DashCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-cmaf,encryption=cenc)",
                  "protocol": "HlsCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/a2fa92c4-77dc-4305-a20e-21c8ad20c8c0/CH1.ism/manifest(format=m3u8-aapl,encryption=cenc)",
                  "protocol": "HlsTs"
                }
              ]
            },
            {
              "streamingLocatorName": "locator-92259edd-db65",
              "streamingPolicyName": "Predefined_ClearStreamingOnly",
              "cencKeyId": null,
              "cbcsKeyId": null,
              "drm": [],
              "urls": [
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest",
                  "protocol": "SmoothStreaming"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-csf)",
                  "protocol": "DashCsf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=mpd-time-cmaf)",
                  "protocol": "DashCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-cmaf)",
                  "protocol": "HlsCmaf"
                },
                {
                  "url": "https://customerssrlsvdeveuwe-customerssrlivedeveuwe-euwe.streaming.media.azure.net/3405a404-268b-4d15-ac15-8c8779e555ca/CH1.ism/manifest(format=m3u8-aapl)",
                  "protocol": "HlsTs"
                }
              ]
            }
          ]
        }
      ]
    }
```

