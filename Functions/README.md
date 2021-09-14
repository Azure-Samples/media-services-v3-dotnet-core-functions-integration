---
services: media-services,functions,github
platforms: dotnetcore
author: xpouyat
---

# .NET 5 Functions for Azure Media Services v3

This project contains examples of Azure Functions that connect to Azure Media Services v3 for video processing. Functions are developped using C# and .NET 5.
You can use Visual Studio 2019 or Visual Studio Code to run and deploy them to Azure. Deployment can also be done using [an Azure Resource Manager (ARM) template and GitHub Actions](#b-second-option--deploy-using-an-arm-template-and-github-actions).

![Deployment architecture using ARM and GitHub Actions](../Images/DrawingAzureFunctionsNet5.png?raw=true)

There are several functions and more will be added in the future. As an example, the **SubmitEncodingJob** function takes a Media Services asset or a source URL and launches an encoding job with Media Services. It uses a Transform which is created if it does not exist. When it is created, it used the preset provided in the input body.
Functions are documented at [the end](#code-documentation).

We recommend to use **[Manage Identity](https://docs.microsoft.com/en-us/azure/media-services/latest/concept-managed-identities)** to authenticate the Azure Functions against Media Services. The other (legacy) option is to use a Service Principal (client Id and client secret). The documentation below supports both methods. Note that a service principal is needed if you want to test your functions locally.

## Prerequisites

### 1. Create an Azure Media Services account

Create a Media Services account in your subscription if don't have it already ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/create-account-howto?tabs=portal)).
If you use Managed Identity, make sure to select "Managed identity / System-managed" in advanced settings.

### 2. Optional : create a Service Principal

If you don't plan to use Managed Identity or if you want to test your functions locally, create a Service Principal and save the password. It will be needed in step #4. To do so, go to the API tab in the account ([follow this article](https://docs.microsoft.com/en-us/azure/media-services/latest/access-api-howto?tabs=portal)).

### 3. Make sure the AMS streaming endpoint is started

To enable streaming, go to the Azure portal, select the Azure Media Services account which has been created, and start the default streaming endpoint.

### 4. Install VS Code or Visual Studio

- Install [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio 2019](https://visualstudio.microsoft.com/).
- [.NET Core 3.1 and .NET 5.0 SDKs](https://dotnet.microsoft.com/download/dotnet).
- The [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) for Visual Studio Code.

## .NET solution file and how to launch project

Open the root `/Functions/Functions.sln` (or just open the Functions folder in VS Code).
The main solution contains the Azure Functions project.

For more on information on .NET 5 & Azure Functions, see [this repository](https://github.com/Azure/azure-functions-dotnet-worker).

### How to set variables for a local execution

Create a **.env file** at the root with your account settings.
For more information, see [Access APIs](https://docs.microsoft.com/en-us/azure/media-services/latest/access-api-howto).

Use [sample.env](../sample.env) as a template for the .env file to be created. The .env file must be placed at the root of the sample (same location than sample.env).
Connect to the Azure portal with your browser and go to your media services account / API access to get the .ENV data to store to the .env file.

As an alternative, you can edit the `local.settings.json` file. In that case, make sure to exclude the file from source control.

### How to test and run the Functions locally

Use Visual Studio or [Visual Studio Code](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#run-the-function-locally) to run the functions.
When using VS Code, you can launch the Functions in the Debugger console (Ctrl-shift-D).

![Screen capture](../Images/azfunc5runvscode.png?raw=true)

Use Postman to test your local functions.

![Screen capture](../Images/azfunc5postman.png?raw=true)

## How to deploy the functions to Azure

In this document, we propose two options to deploy the resources (Storage, Plan and Functions App) and deploy the code to the Azure Functions app.

- Option (A) : using Visual Studio Code or Azure CLI
- Option (B) : using an ARM template to deploy the infrastructure, and GitHub Actions for continuous deployment (CD). With such deployment, the Azure app instance will be automatically updated when you commit code to your repo.

### Option (A): deploy from Visual Studio Code or Azure CLI

For Azure CLI commands, read [this](https://github.com/Azure/azure-functions-dotnet-worker).

With VS Code, select the Azure tab and click on the icon to deploy it to Azure.

![Screen capture](../Images/azfunc5deploy.png?raw=true)

Once the deployment is done, to get the function Url, use the Functions explorer in VSCode, or the Azure portal.

![Screen capture](../Images/azfunc5geturl.png?raw=true)

More details in the [documentation](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-csharp?tabs=in-process#sign-in-to-azure).

Once deployed, go the Azure portal, select your Azure functions app, go to the 'Configuration' tab, select 'Advanced Edit' and add the following entries with your own values.

```json
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

#### If a Service Principal is used

Add these two entries in the configuration of the Azure Function app (and replace with your own values):

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
  }
```

#### If Managed Identity is used

- Go to the portal and select your Media Services account.
- Go to `Access Control (IAM)` to the left
- Select `Add` (on the top) / `Add role assignment`
- Select role `Media Services Media Operator`
- Select `Function App` in the box `Assign access to`
- You should see your Function app name. Select it and select `Save`.

![Screen capture](../Images/azfunc5roleassignment.png?raw=true)

- If you want to operate Live Events with your functions, repeat the previous steps to add role `Media Services Live Events Administrator`.

Restart the Function App.

#### Testing

You can use Postman to test your Azure functions.

![Screen capture](../Images/azfunc5postmandeployed.png?raw=true)

### Option (B): deploy using an ARM template and GitHub Actions

In this section, we deploy and configure an Azure Functions App using an ARM template, then we will configure your GitHub repo to push code updates to the Function App using GitHub Actions.

#### (B.1) Fork the repo

If not already done : fork the repo.

#### (B.2) Managed Identity option: deploy the resources using ARM

If you plan to use **Managed Identity**, you can deploy the Azure resources using the ARM template [`azuredeploy2mi.json`](azuredeploy2mi.json).

Click on this button to deploy the resources to your subscription :

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmain%2FFunctions%2Fazuredeploy2mi.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

When the deployment is complete, you need to grant the function app access to the Media Services account resource. Go to your new Functions App and get the `Object (principal) ID` :

1. In Azure portal, go to your function app, then `Identity` tab.
2. In the `System assigned` section, copy the `Object (principal) ID` value.

Now run the commands below in a Terminal. For this request:

- `assignee` is the `Object (principal) ID`
- `scope` is the `id` of your Media Services account.

```azurecli-interactive
az login

az role assignment create --assignee 00000000-0000-0000-000000000000 --role "Media Services Live Events Administrator" --scope "/subscriptions/<the-subscription-id>/resourceGroups/<your-resource-group>/providers/Microsoft.Media/mediaservices/<your-media-services-account-name>"

az role assignment create --assignee 00000000-0000-0000-000000000000 --role "Media Services Media Operator" --scope "/subscriptions/<the-subscription-id>/resourceGroups/<your-resource-group>/providers/Microsoft.Media/mediaservices/<your-media-services-account-name>
```

If you cannot run the commands from a terminal, you can do it from the portal. Follow the section [If Managed Identity is used](#If-Managed-Identity-is-used).

#### (B.2) Service Principal option: deploy the resources using ARM

If you plan to use a **Service Principal**, you can deploy the Azure resources using the ARM template [`azuredeploy2.json`](azuredeploy2.json).

Click on this button to deploy the resources to your subscription :

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fmedia-services-v3-dotnet-core-functions-integration%2Fmain%2FFunctions%2Fazuredeploy2.json" target="_blank"><img src="http://azuredeploy.net/deploybutton.png"/></a>

#### (B.3) Use Publish Profile as Deployment Credential

When the deployment is complete, go to your new Functions App to get the publish profile and store it as a secret in your GitHub repository.

1. In Azure portal, go to your function app, then Deployment Center tab.
2. Click **Manage publish profile**, **Download publish profile** to download **.PublishSettings** file.
3. Open the **.PublishSettings** file and copy the content.
4. Paste the XML content to your GitHub Repository > Settings > Secrets > Add a new secret > **AZURE_FUNCTIONAPP_PUBLISH_PROFILE**

#### (B.4) Setup Continuous deployment

Let's customize the workflow file to enable continuous deployment (CD) with GitHub Actions.

1. Edit the [`.github/workflows/deploy-functions.yml`](../.github/workflows/deploy-functions.yml) file in your project repository. This file is the workflow for GitHub Actions.
2. Change variable value **AZURE_FUNCTIONAPP_NAME** in `env:` section according to your function app name.
3. Commit the change.
4. Still in GitHub, go to **Actions**. Enable the workflows if they are disabled (worklows are disabled when they come from the source repo used by the fork).
5. You should see a new GitHub workflow initiated in **Actions** tab, called **Build and deploy dotnet 5 app to Azure Function App**.

[`deploy-functions.yml`](../.github/workflows/deploy-functions.yml) based file :

```yml
name: Build and deploy dotnet 5 app to Azure Function App

on:
  push:
#    branches:
#      - main
#  workflow_dispatch:

# CONFIGURATION
# For help, go to https://github.com/Azure/Actions
#
# 1. Set up the following secrets in your repository:
#   AZURE_FUNCTIONAPP_PUBLISH_PROFILE
#
# 2. Change these variables for your configuration:
env:
  AZURE_FUNCTIONAPP_NAME: amsv3functionsxxxxxxxxxxxx  # set this to your application's name
  AZURE_FUNCTIONAPP_PACKAGE_PATH: 'Functions'    # set this to the path to your web app project, set to '.' to use repository root
  DOTNET_VERSION: '5.0.400'              # set this to the dotnet version to use
  DOTNET_VERSION_WORKER: '3.1.409'       # set this to the dotnet version to use to build the the Functions Worker Extension

jobs:
  build-and-deploy:
    runs-on: windows-latest
    environment: dev
    steps:
    - name: 'Checkout GitHub Action'
      uses: actions/checkout@master

    - name: Setup DotNet ${{ env.DOTNET_VERSION_WORKER }} Function Worker Environment
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ env.DOTNET_VERSION_WORKER }}

    - name: Setup DotNet ${{ env.DOTNET_VERSION }} Environment
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: 'Resolve Project Dependencies Using Dotnet'
      shell: pwsh
      run: |
        pushd './${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}'
        dotnet build --configuration Release --output ./output
        popd

    - name: 'Run Azure Functions Action'
      uses: Azure/functions-action@v1
      id: fa
      with:
        app-name: ${{ env.AZURE_FUNCTIONAPP_NAME }}
        package: '${{ env.AZURE_FUNCTIONAPP_PACKAGE_PATH }}/output'
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}

# For more samples to get started with GitHub Action workflows to deploy to Azure, refer to https://github.com/Azure/actions-workflow-samples
```

Note : This script installs two versions of .NET and asks the engine to deploy the code from the `/Functions` folder of the repo.

![Screen capture](../Images/azfunc5githubactions.png?raw=true)

Check the status of the workflow in GitHub Actions. If everything worked fine, you should see the functions in the portal.

![Screen capture](../Images/azfunc5appinstance.png?raw=true)

#### (B.5) Test the functions

In the portal, you can now get the URLs for your functions.

![Screen capture](../Images/azfunc5geturlportal.png?raw=true)

Run Postman to test your Azure functions.

![Screen capture](../Images/azfunc5postmandeployed.png?raw=true)

For more information on this type of deployment, see [GitHub Actions for deploying to Azure Functions](https://github.com/Azure/functions-action).

## Code documentation

### CreateEmptyAsset

This function creates a Media Services asset. It returns back the asset name, asset Id and storage container.
See the model for the [input](CreateEmptyAsset.cs#L20-L47) and [output](CreateEmptyAsset.cs#L49-L71).

Input body sample :

```json
{
    "assetNamePrefix": "test"
}
 ```

Or

```json
{
    "assetNamePrefix": "test",
    "assetDescription" : "a new asset created by a function",
    "assetStorageAccount" : "storageams01"
}
 ```

### DeleteAsset

This function deletes a Media Services asset.
See the model for the [input](DeleteAsset.cs#L20-L31).

Input body sample :

```json
{
    "assetName": "test"
}
```

### SubmitEncodingJob

This function processes a Media Services asset or a source URL. It launches a job using the Transform name provided in the input. If the Transform does not exist, the function creates one based on the provided Media Encoder Standard preset. The function returns back the output asset name and job name.
See the model for the [input](SubmitEncodingJob.cs#L22-L63) and [output](SubmitEncodingJob.cs#L65-L81).

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
