//
// Azure Media Services REST API v3 Functions
//
// CreateContentKeyPolicy - This function creates an ContentKeyPolicy object.
//
/*
```c#
Input:
    {
        // [Required] The content key policy name.
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",

        // [Required] The mode for creating the transform.
        // Allowed values: "simple" or "advanced".
        // Default value: "simple".
        "mode": "simple",

        // The content key policy description.
        "description": "Shared toekn restricted policy for Clear Key content key policy",

        //
        // [mode = simple]
        //
        // [Required] The content key policy option name
        "policyOptionName": "CommonEncryptionPlayReadyTokenRestrictedOption",

        //
        // [mode = simple]
        // Restriction Arguments
        //
        // [Required] Use Open Restriction.
        // License or key will be delivered on every request without restrictions.
        // Not recommended for production environments.
        // Allowed values: true or false
        // Default value: false
        "openRestriction": false,
        //
        // Token Restricted options (openRestriction = false)
        //
        // The audience for the token.
        "audience": "urn:myAudience",
        // The token issuer.
        "issuer": "urn:myIssuer",
        // The type of token. Allowed values: Jwt, Swt.
        "tokenType": "Jwt",
        // The type of the token key to be used for the primary verification key.
        // Allowed values: Symmetric, RSA, X509.
        "tokenKeyType": "Symmetric",
        // The token key Base64 string for symmetric key or certificate (x509) or public key (rsa).
        // Must be used in conjunction with --token-key-type.
        "tokenKey": "AAAAAAAAAAAAAAAAAAAAAA=="
        // Semi-colon-separated required token claims in '[key=value]' format.
        "tokenClaims": "urn:microsoft:azure:mediaservices:contentkeyidentifier=null",
        // Semi-colon-separated list of alternate rsa token keys.
        "altRsaTokenKeys": null,
        // Semi-colon-separated list of alternate symmetric token keys.
        "altSymmetricTokenKeys": null,
        // Semi-colon-separated list of alternate x509 certificate token keys.
        "altX509TokenKeys": null,
        // The OpenID connect discovery document.
        "openIdConnectDiscoveryDocument": null,
        
        //
        // [mode = simple]
        // ContentKeyPolicyConfiguration Arguments
        //
        // The type of content key policy option configuration
        // Allowed values: ClearKey, FairPlay, PlayReady, Widevine
        "configurationType": "ClearKey",
        // FairPlay:
        // The Base64 string of the key that must be used as FairPlay Application Secret key.
        "fairPlayAsk": "AAAAAAAAAAAAAAAAAAAAAA==",
        // The Base64 string if a FairPlay certificate file in PKCS 12 (pfx) format (including private key).
        "fairPlayPfx": "AAAAAAAAAAAAAAAAAAAAAA=="
        // The password encrypting FairPlay certificate in PKCS 12 (pfx) format.
        "fairPlayPfxPassword": "xxx"
        // The rental and lease key type.
        // Available values: Undefined, PersistentUnlimited, PersistentLimited.
        "faiPlayRentalAndLeaseKeyType": "Undefined",
        // The rental duration. Must be greater than or equal to 0.
        "faiPlayRentalDuration": 0,
        // PlayReady:
        // JSON data: the list of PlayReady license template.
        "playReadyTemplates": [ { ... } ],
        // The string data of PlayReady response custom data.
        "playReadyResponseCustomData": "xxx",
        // Widevine:
        // JSON Widevine license template.
        "widevineTemplate": {},

        //
        // [mode = advanced]
        //
        // The JSON data for options of the content key policy.
        // You can create multiple options in this 
        "contentKeyPolicyOptions": [
            {
                "name": "ClearKeyOption",
                "configuration": {
                    "@odata.type": "#Microsoft.Media.ContentKeyPolicyClearKeyConfiguration"
                },
                "restriction": {
                    "@odata.type": "#Microsoft.Media.ContentKeyPolicyTokenRestriction",
                    "issuer": "urn:issuer",
                    "audience": "urn:audience",
                    "primaryVerificationKey": {
                        "@odata.type": "#Microsoft.Media.ContentKeyPolicySymmetricTokenKey",
                        "keyValue": "AAAAAAAAAAAAAAAAAAAAAA=="
                    },
                    "restrictionTokenType": "Swt"
                }
            }
        ]
    }
Output:
    {
        // The name of the content key policy.
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",
        // The identifier of the content key policy.
        "contentKeyPolicyId": "9d6a2b92-d61a-4e87-8348-7155c137f9ca",
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using advanced_vod_functions_v3.SharedLibs;


namespace advanced_vod_functions_v3
{
    public static class CreateContentKeyPolicy
    {
        [FunctionName("CreateContentKeyPolicy")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info($"AMS v3 Function - CreateContentKeyPolicy was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (data.contentKeyPolicyName == null)
                return new BadRequestObjectResult("Please pass contentKeyPolicyName in the input object");
            string contentKeyPolicyName = data.contentKeyPolicyName;
            string contentKeyPolicyDescription = null;
            if (data.contentKeyPolicyDescription == null)
                contentKeyPolicyDescription = data.contentKeyPolicyDescription;

            string mode = data.mode;
            if (mode != "simple" && mode != "advanced")
                return new BadRequestObjectResult("Please pass valid mode in the input object");

            //
            // Simple Mode
            //
            if (mode == "simple")
            {
                if (data.policyOptionName == null)
                    return new BadRequestObjectResult("Please pass policyOptionName in the input object");
                if (data.openRestriction == null)
                    return new BadRequestObjectResult("Please pass openRestriction in the input object");
                if (data.openRestriction == false)
                    if (data.audience == null
                        && data.issuer == null
                        && data.tokenType == null
                        && data.tokenKeyType == null
                        && data.tokenKey == null)
                        return new BadRequestObjectResult("Please pass required parameters for Token Restriction in the input object");

            }
            //
            // Advanced Mode
            //
            if (mode == "advanced")
            {
                if (data.contentKeyPolicyOptions == null)
                    return new BadRequestObjectResult("Please pass contentKeyPolicyOptions in the input object");
            }

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            ContentKeyPolicy policy = null;

            JsonConverter[] jsonReaders = {
                new MediaServicesHelperJsonReader(),
                new MediaServicesHelperTimeSpanJsonConverter()
            };

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                policy = client.ContentKeyPolicies.Get(amsconfig.ResourceGroup, amsconfig.AccountName, contentKeyPolicyName);

                if (policy == null)
                {
                    List<ContentKeyPolicyOption> options = new List<ContentKeyPolicyOption>();

                    if (mode == "simple")
                    {
                        ContentKeyPolicyOption option = new ContentKeyPolicyOption();
                        option.Name = data.policyOptionName;

                        // Restrictions
                        if (data.openRestriction)
                            option.Restriction = new ContentKeyPolicyOpenRestriction();
                        else
                        {
                            ContentKeyPolicyTokenRestriction restriction = new ContentKeyPolicyTokenRestriction();
                            restriction.Audience = data.audience;
                            restriction.Issuer = data.issuer;
                            switch (data.tokenType)
                            {
                                case "Jwt":
                                    restriction.RestrictionTokenType = ContentKeyPolicyRestrictionTokenType.Jwt;
                                    break;
                                case "Swt":
                                    restriction.RestrictionTokenType = ContentKeyPolicyRestrictionTokenType.Swt;
                                    break;
                                default:
                                    return new BadRequestObjectResult("Please pass valid tokenType in the input object");
                            }
                            switch (data.tokenKeyType)
                            {
                                case "Symmetric":
                                    restriction.PrimaryVerificationKey = new ContentKeyPolicySymmetricTokenKey(Convert.FromBase64String(data.tokenKey.ToString()));
                                    break;
                                case "X509":
                                    restriction.PrimaryVerificationKey = new ContentKeyPolicyX509CertificateTokenKey(Convert.FromBase64String(data.tokenKey.ToString()));
                                    break;
                                case "RSA":
                                    restriction.PrimaryVerificationKey = new ContentKeyPolicyRsaTokenKey();
                                    break;
                                default:
                                    return new BadRequestObjectResult("Please pass valid tokenKeyType in the input object");
                            }
                            if (data.tokenClaims)
                            {
                                restriction.RequiredClaims = new List<ContentKeyPolicyTokenClaim>();
                                String[] tokenClaims = data.tokenClaims.ToString().Split(';');
                                foreach (string tokenClaim in tokenClaims)
                                {
                                    String[] tokenClaimKVP = tokenClaim.Split('=');
                                    if (tokenClaimKVP.Length > 1)
                                        return new BadRequestObjectResult("Please pass valid tokenClaims in the input object");
                                    restriction.RequiredClaims.Add(new ContentKeyPolicyTokenClaim(tokenClaimKVP[0], tokenClaimKVP[1]));
                                }
                            }
                            if (data.openIdConnectDiscoveryDocument != null)
                                restriction.OpenIdConnectDiscoveryDocument = data.openIdConnectDiscoveryDocument;

                            option.Restriction = restriction;
                        }
                        // Configuration
                        switch (data.configurationType)
                        {
                            case "ClearKey":
                                option.Configuration = new ContentKeyPolicyClearKeyConfiguration();
                                break;
                            case "FairPlay":
                                ContentKeyPolicyFairPlayConfiguration configFairPlay = new ContentKeyPolicyFairPlayConfiguration();
                                configFairPlay.Ask = Convert.FromBase64String(data.fairPlayAsk);
                                configFairPlay.FairPlayPfx = data.fairPlayPfx;
                                configFairPlay.FairPlayPfxPassword = data.fairPlayPfxPassword;
                                switch (data.faiPlayRentalAndLeaseKeyType)
                                {
                                    case "Undefined":
                                        configFairPlay.RentalAndLeaseKeyType = ContentKeyPolicyFairPlayRentalAndLeaseKeyType.Undefined;
                                        break;
                                    case "PersistentLimited":
                                        configFairPlay.RentalAndLeaseKeyType = ContentKeyPolicyFairPlayRentalAndLeaseKeyType.PersistentLimited;
                                        break;
                                    case "PersistentUnlimited":
                                        configFairPlay.RentalAndLeaseKeyType = ContentKeyPolicyFairPlayRentalAndLeaseKeyType.PersistentUnlimited;
                                        break;
                                    default:
                                        return new BadRequestObjectResult("Please pass valid faiPlayRentalAndLeaseKeyType in the input object");
                                }
                                configFairPlay.RentalDuration = data.faiPlayRentalDuration;
                                break;
                            case "PlayReady":
                                ContentKeyPolicyPlayReadyConfiguration configPlayReady = new ContentKeyPolicyPlayReadyConfiguration();
                                configPlayReady.Licenses = JsonConvert.DeserializeObject<List<ContentKeyPolicyPlayReadyLicense>>(data.playReadyTemplates.ToString(), jsonReaders);
                                if (data.playReadyResponseCustomData != null) configPlayReady.ResponseCustomData = data.playReadyResponseCustomData;
                                option.Configuration = configPlayReady;
                                break;
                            case "Widevine":
                                ContentKeyPolicyWidevineConfiguration configWideVine = JsonConvert.DeserializeObject<ContentKeyPolicyWidevineConfiguration>(data.widevineTemplate.ToString(), jsonReaders);
                                option.Configuration = configWideVine;
                                break;
                            default:
                                return new BadRequestObjectResult("Please pass valid configurationType in the input object");
                        }
                        options.Add(option);
                    }
                    else if (mode == "advanced")
                    {
                        options = JsonConvert.DeserializeObject<List<ContentKeyPolicyOption>>(data.contentKeyPolicyOptions.ToString(), jsonReaders);
                    }

                    foreach (ContentKeyPolicyOption o in options)
                        o.Validate();
                    policy = client.ContentKeyPolicies.CreateOrUpdate(amsconfig.ResourceGroup, amsconfig.AccountName, contentKeyPolicyName, options, contentKeyPolicyDescription);
                }
            }
            catch (ApiErrorException e)
            {
                log.Info($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.Info($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                policyId = policy.PolicyId
            });
        }
    }
}
