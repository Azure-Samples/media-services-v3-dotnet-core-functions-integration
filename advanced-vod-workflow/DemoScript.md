# Publish Asset with AES Open Restriction

## Script
### 1. POST CreateContentKeyPolicy
{
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKeyOpenRestriction",
    "mode": "simple",
    "description": "Shared token restricted policy for Clear Key content key policy",
    "policyOptionName": "PolicyWithClearKeyOpenRestrictionOption",
    "openRestriction": true,
    "configurationType": "ClearKey"
}

### 2. POST PublishAsset
{
    "assetName": "RollsRoyceIoT-048cf514-6337-430a-b538-94b1727dbc91",
    "streamingPolicyName": "Predefined_ClearKey",
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKeyOpenRestriction",
    "contentKeys": [
        {
            "id": "271b37ed-6098-4a16-8e16-c3beb79c6199",
            "type": "EnvelopeEncryption",
            "labelReferenceInStreamingPolicy": "clearKeyDefault",
            "value": "on1W311b4SiZ9c2KCDIy8w==",
            "policyName": null,
            "tracks": []
        }
    ],
    "startDateTime": "2020-04-20T00:00Z",
    "endDateTime": "2025-12-31T23:59Z"
}

## Clean Up
### 1. POST UnpublishAsset
{
    "streamingLocatorName": "streaminglocator-6bcfd46d-ee30-4c4f-bd0a-9852a00a0452"
}

### 2. POST DeleteContentKeyPolicy
{
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKeyOpenRestriction",
}

# Publish Asset with AES Jwt Token Restriction

## Script
### 1. POST CreateContentKeyPolicy
{
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
    "mode": "advanced",
    "description": "Shared token restricted policy for Clear Key content key policy",
    "contentKeyPolicyOptions": [
        {
            "name": "PolicyWithClearKeyOption",
            "configuration": {
                "@odata.type": "#Microsoft.Media.ContentKeyPolicyClearKeyConfiguration"
            },
            "restriction": {
                "@odata.type": "#Microsoft.Media.ContentKeyPolicyTokenRestriction",
                "issuer": "myIssuer",
                "audience": "myAudience",
                "primaryVerificationKey": {
                    "@odata.type": "#Microsoft.Media.ContentKeyPolicySymmetricTokenKey",
                    "keyValue": "on1W311b4SiZ9c2KCDIy8w=="
                },
                "alternateVerificationKeys": [],
                "requiredClaims": [
                    {
                        "claimType": "urn:microsoft:azure:mediaservices:contentkeyidentifier",
                        "claimValue": null
                    }
                ],
                "restrictionTokenType": "Jwt",
                "openIdConnectDiscoveryDocument": null
            }
        }
    ]
}

### 2. POST PublishAsset
{
    "assetName": "AzureMediaServices-ba6aad15-0417-44d7-b778-159b12adb990",
    "streamingPolicyName": "Predefined_ClearKey",
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
    "contentKeys": [
        {
            "id": "1d5f3e32-7314-46ac-a713-3e066d289ed7",
            "type": "EnvelopeEncryption",
            "labelReferenceInStreamingPolicy": "clearKeyDefault",
            "value": "njjOFXLDn3RLfkKTdnrwZQ==",
            "policyName": null,
            "tracks": []
        }
    ],
    "startDateTime": "2020-04-20T00:00Z",
    "endDateTime": "2025-12-31T23:59Z"
}

## Clean Up
### 1. POST UnpublishAsset
{
    "streamingLocatorName": "streaminglocator-0705f511-879a-4258-9ce2-ca0e13c31995"
}

### 2. POST DeleteContentKeyPolicy
{
    "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey"
}