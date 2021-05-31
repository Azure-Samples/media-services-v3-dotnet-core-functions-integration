---
services: media-services,logic-app
platforms: dotnetcore
author: raffertyuy
---

# Media Services: Integrating Azure Media Services with Azure Functions & Azure Logic Apps

This project contains Logic Apps that converts-to-media-asset, encodes and publishes media files that you upload in an Azure Storage.

The project includes Logic Apps ARM templates that is dependent on the [advanced-vod-workflow](../advanced-vod-workflow) functions. There are 2 projects, one for uploading and encoding and another for publishing.

This Media Services Functions example is based on AMS REST API v3 on Azure Functions v3.

See [here](https://raffertyuy.com/raztype/creating-an-azure-media-services-upload-workflow-using-azure-storage-and-logic-apps) to read more about how this logic app is implemented.

## Prerequisites for a sample Logic Apps deployments

### 1. Deploy the Advanced VOD Workflow Azure Functions

Follow the instructions and deploy the azure functions found in [advanced-vod-workflow](../advanced-vod-workflow).

### 2. Edit the ARM Template Parameters

Update the parameters found in the LogicApp.json of both projects

### 3. Deploy the 2 Logic App Projects

See [here](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-azure-resource-manager-templates-overview) for instructions.
