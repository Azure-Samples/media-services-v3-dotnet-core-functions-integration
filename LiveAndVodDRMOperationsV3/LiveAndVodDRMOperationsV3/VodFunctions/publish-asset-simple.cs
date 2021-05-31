//
// Azure Media Services REST API v3 Functions
//
// publish-asset--simple -  This function publish an asset for dynamic encryption (to be used with Irdeto)
//
/*
```c#
Input :
{
    "assetName": "asset-hgdhfg56",
    "azureRegion": "euwe" or "we" or "euno" or "no" or "euwe,euno" or "we,no"
            // optional. If this value is set, then the AMS account name and resource group are appended with this value.
            // Resource name is not changed if "ResourceGroupFinalName" in app settings is to a value non empty.
            // This feature is useful if you want to manage several AMS account in different regions.
            // if two regions are sepecified using a comma as a separator, then the function will operate in the two regions at the same time. With this function, two live events will be created.
            // Note: the service principal must work with all this accounts
}


Output:
{
  "success": true,
  "operationsVersion": "1.0.1.0",
  "createdLocatorName": "locator-4d8d939a-5346",
  "createdLocatorPath": "https//customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/",
  "asset": {
    "assetName": "92ccbf1edb-input-59894-ContentAwareEncode-output-59894",
    "assetStorageAccountName": "customeraccountstorage",
    "streamingLocators": [
      {
        "streamingLocatorName": "locator-ecf1f451-5848",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/ecf1f451-5848-4ec9-9553-02169722efc0/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/ecf1f451-5848-4ec9-9553-02169722efc0/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/ecf1f451-5848-4ec9-9553-02169722efc0/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/ecf1f451-5848-4ec9-9553-02169722efc0/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/ecf1f451-5848-4ec9-9553-02169722efc0/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-84bab94d-9670",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/84bab94d-9670-4a6e-9461-37d1d1bd5f92/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/84bab94d-9670-4a6e-9461-37d1d1bd5f92/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/84bab94d-9670-4a6e-9461-37d1d1bd5f92/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/84bab94d-9670-4a6e-9461-37d1d1bd5f92/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/84bab94d-9670-4a6e-9461-37d1d1bd5f92/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-5f800dfe-fd61",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/5f800dfe-fd61-42f8-a144-40662c0c7c65/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/5f800dfe-fd61-42f8-a144-40662c0c7c65/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/5f800dfe-fd61-42f8-a144-40662c0c7c65/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/5f800dfe-fd61-42f8-a144-40662c0c7c65/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/5f800dfe-fd61-42f8-a144-40662c0c7c65/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-22e826e9-5f0d",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/22e826e9-5f0d-410c-a037-4d59ba001478/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/22e826e9-5f0d-410c-a037-4d59ba001478/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/22e826e9-5f0d-410c-a037-4d59ba001478/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/22e826e9-5f0d-410c-a037-4d59ba001478/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/22e826e9-5f0d-410c-a037-4d59ba001478/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-e579749b-4258",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/e579749b-4258-4ab0-853c-4e192f5a4e50/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/e579749b-4258-4ab0-853c-4e192f5a4e50/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/e579749b-4258-4ab0-853c-4e192f5a4e50/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/e579749b-4258-4ab0-853c-4e192f5a4e50/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/e579749b-4258-4ab0-853c-4e192f5a4e50/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-0bda2b83-505f",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/0bda2b83-505f-49aa-87d8-4e7bdcdbd8a7/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/0bda2b83-505f-49aa-87d8-4e7bdcdbd8a7/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/0bda2b83-505f-49aa-87d8-4e7bdcdbd8a7/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/0bda2b83-505f-49aa-87d8-4e7bdcdbd8a7/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/0bda2b83-505f-49aa-87d8-4e7bdcdbd8a7/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-47301bf4-1d7a",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/47301bf4-1d7a-4617-affb-6622ef2606b5/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/47301bf4-1d7a-4617-affb-6622ef2606b5/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/47301bf4-1d7a-4617-affb-6622ef2606b5/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/47301bf4-1d7a-4617-affb-6622ef2606b5/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/47301bf4-1d7a-4617-affb-6622ef2606b5/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-efa65560-22c9",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/efa65560-22c9-4f22-8bf6-67ebdbb86c82/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/efa65560-22c9-4f22-8bf6-67ebdbb86c82/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/efa65560-22c9-4f22-8bf6-67ebdbb86c82/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/efa65560-22c9-4f22-8bf6-67ebdbb86c82/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/efa65560-22c9-4f22-8bf6-67ebdbb86c82/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-3fefa123-41ae",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3fefa123-41ae-4044-ae0f-79436859a735/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3fefa123-41ae-4044-ae0f-79436859a735/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3fefa123-41ae-4044-ae0f-79436859a735/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3fefa123-41ae-4044-ae0f-79436859a735/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3fefa123-41ae-4044-ae0f-79436859a735/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-f77ee090-8f70",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/f77ee090-8f70-47ee-99df-8a5056c6f691/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/f77ee090-8f70-47ee-99df-8a5056c6f691/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/f77ee090-8f70-47ee-99df-8a5056c6f691/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/f77ee090-8f70-47ee-99df-8a5056c6f691/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/f77ee090-8f70-47ee-99df-8a5056c6f691/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-08459d89-dbc5",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/08459d89-dbc5-4625-a7a5-9e585d94af0e/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/08459d89-dbc5-4625-a7a5-9e585d94af0e/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/08459d89-dbc5-4625-a7a5-9e585d94af0e/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/08459d89-dbc5-4625-a7a5-9e585d94af0e/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/08459d89-dbc5-4625-a7a5-9e585d94af0e/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-4d8d939a-5346",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/4d8d939a-5346-4c69-b91f-a5ee5490594f/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "92ccbf1edb-input-59894-ContentAwareEncode-output-59894-locator",
        "streamingPolicyName": "Predefined_ClearKey",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/a9b70c8e-dc32-4da7-ac64-a8d781fbf8b2/BBB_trailer_202.ism/manifest(encryption=cenc)"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/a9b70c8e-dc32-4da7-ac64-a8d781fbf8b2/BBB_trailer_202.ism/manifest(format=mpd-time-csf,encryption=cenc)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/a9b70c8e-dc32-4da7-ac64-a8d781fbf8b2/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf,encryption=cenc)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/a9b70c8e-dc32-4da7-ac64-a8d781fbf8b2/BBB_trailer_202.ism/manifest(format=m3u8-cmaf,encryption=cbcs-aapl)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/a9b70c8e-dc32-4da7-ac64-a8d781fbf8b2/BBB_trailer_202.ism/manifest(format=m3u8-aapl,encryption=cbcs-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-98b6a43f-d566",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/98b6a43f-d566-48ef-a971-a8e937e30354/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/98b6a43f-d566-48ef-a971-a8e937e30354/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/98b6a43f-d566-48ef-a971-a8e937e30354/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/98b6a43f-d566-48ef-a971-a8e937e30354/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/98b6a43f-d566-48ef-a971-a8e937e30354/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-d613427b-52ae",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/d613427b-52ae-49b6-b7e4-ae5462195c8e/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/d613427b-52ae-49b6-b7e4-ae5462195c8e/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/d613427b-52ae-49b6-b7e4-ae5462195c8e/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/d613427b-52ae-49b6-b7e4-ae5462195c8e/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/d613427b-52ae-49b6-b7e4-ae5462195c8e/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      },
      {
        "streamingLocatorName": "locator-3bdc43ce-1b25",
        "streamingPolicyName": "Predefined_DownloadAndClearStreaming",
        "cencKeyId": null,
        "cbcsKeyId": null,
        "drm": [],
        "urls": [
          {
            "protocol": "SmoothStreaming",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3bdc43ce-1b25-4e04-bd78-c00b68015c31/BBB_trailer_202.ism/manifest"
          },
          {
            "protocol": "DashCsf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3bdc43ce-1b25-4e04-bd78-c00b68015c31/BBB_trailer_202.ism/manifest(format=mpd-time-csf)"
          },
          {
            "protocol": "DashCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3bdc43ce-1b25-4e04-bd78-c00b68015c31/BBB_trailer_202.ism/manifest(format=mpd-time-cmaf)"
          },
          {
            "protocol": "HlsCmaf",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3bdc43ce-1b25-4e04-bd78-c00b68015c31/BBB_trailer_202.ism/manifest(format=m3u8-cmaf)"
          },
          {
            "protocol": "HlsTs",
            "url": "https://customeraccount-euwe.streaming.media.azure.net/3bdc43ce-1b25-4e04-bd78-c00b68015c31/BBB_trailer_202.ism/manifest(format=m3u8-aapl)"
          }
        ]
      }
    ],
    "amsAccountName": "customeraccount",
    "region": "West Europe",
    "resourceGroup": "DimensionInsightResourceGroup",
    "createdTime": "20200410T11:20:35Z",
    "id": "customeraccount:92ccbf1edb-input-59894-contentawareencode-output-59894",
    "partitionKey": "und"
  }
}
```
*/

using LiveDrmOperationsV3.Helpers;
using LiveDrmOperationsV3.Models;
using LiveDRMOperationsV3.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3
{
    public static class PublishAssetSimple
    {
        // This version registers keys in irdeto backend. For FairPlay and rpv3

        [FunctionName("publish-asset-simple")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            dynamic data;
            try
            {
                data = JsonConvert.DeserializeObject(new StreamReader(req.Body).ReadToEnd());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }


            var assetName = (string)data.assetName;
            if (assetName == null)
                return IrdetoHelpers.ReturnErrorException(log, "Error - please pass assetName in the JSON");


            // Azure region management
            var azureRegions = new List<string>();
            if ((string)data.azureRegion != null)
            {
                azureRegions = ((string)data.azureRegion).Split(',').ToList();
            }
            else
            {
                azureRegions.Add((string)null);
            }

            // semaphore file (json structure)
            VodSemaphore semaphore = null;
            if (data.semaphore != null)
            {
                semaphore = VodSemaphore.FromJson((string)data.semaphore);
            }


            // init default
            var streamingLocatorGuid = Guid.NewGuid(); // same locator for the two ouputs if 2 live event namle created
            var uniquenessLocator = streamingLocatorGuid.ToString().Substring(0, 13);
            var streamingLocatorName = "locator-" + uniquenessLocator;
            string locatorPath = string.Empty;

            string uniquenessPolicyName = Guid.NewGuid().ToString().Substring(0, 13);

            // Default content id and semaphare value
            string irdetoContentId = null;
            if (semaphore != null && semaphore.DrmContentId != null) // semaphore data has higher priority 
            {
                irdetoContentId = semaphore.DrmContentId;
            }
            else if (data.defaultIrdetoContentId != null)
            {
                irdetoContentId = (string)data.defaultIrdetoContentId;
            }

            var clientTasks = new List<Task<AssetEntry>>();

            foreach (var region in azureRegions)
            {
                var task = Task<AssetEntry>.Run(async () =>
                {
                    Asset asset = null;

                    ConfigWrapper config = new ConfigWrapper(new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddEnvironmentVariables()
                            .Build(),
                            region
                    );

                    MediaServicesHelpers.LogInformation(log, "config loaded.", region);
                    MediaServicesHelpers.LogInformation(log, "connecting to AMS account : " + config.AccountName, region);

                    var client = await MediaServicesHelpers.CreateMediaServicesClientAsync(config);
                    // Set the polling interval for long running operations to 2 seconds.
                    // The default value is 30 seconds for the .NET client SDK
                    client.LongRunningOperationRetryTimeout = 2;

                    // let's get the asset
                    try
                    {
                        MediaServicesHelpers.LogInformation(log, "Getting asset.", region);
                        asset = await client.Assets.GetAsync(config.ResourceGroup, config.AccountName, assetName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error when retreving asset by name", ex);
                    }


                    // Locator creation
                    try
                    {
                        StreamingLocator locator = null;
                        locator = await IrdetoHelpers.CreateClearLocator(config, streamingLocatorName, client, asset, streamingLocatorGuid);

                        MediaServicesHelpers.LogInformation(log, "locator : " + locator.Name, region);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error when creating the locator", ex);
                    }


                    // let's build info for the live event and output

                    AssetEntry assetEntry = await GenerateInfoHelpers.GenerateAssetInformation(config, client, asset, semaphore, irdetoContentId, region);

                    var locatorPath2 = assetEntry.StreamingLocators.Where(l => l.StreamingLocatorName == streamingLocatorName).First().Urls.Where(u => u.Protocol == OutputProtocol.SmoothStreaming.ToString()).First().Url;
                    var uriloc = new Uri(locatorPath2);
                    locatorPath = uriloc.Scheme + "://" + uriloc.Host + "/" + uriloc.Segments[1];

                    if (!await CosmosHelpers.CreateOrUpdateAssetDocument(assetEntry))
                        log.LogWarning("Cosmos access not configured.");

                    return assetEntry;

                });
                clientTasks.Add(task);
            }

            try
            {
                Task.WaitAll(clientTasks.ToArray());
            }
            catch (Exception ex)
            {
                return IrdetoHelpers.ReturnErrorException(log, ex);
            }

            return new OkObjectResult(
                                        JsonConvert.SerializeObject(new VodAssetInfoSimple { CreatedLocatorName = streamingLocatorName, CreatedLocatorPath = locatorPath, Success = true, Asset = clientTasks.Select(i => i.Result).First() }, Formatting.Indented)
                                    );
        }
    }
}