//
// Azure Media Services REST API v3 Functions
//
// CreateJwtToken - Create a Jwt Token
//
/*
```c#
Input:
    {
        // [Required] The name of the streaming locator
        "streamingLocatorName": "locator-12345",

        // [Required] The name of the content key policy
        "contentKeyPolicyName": "SharedContentKeyPolicyForClearKey",

        // [Required] The name of the Azure Media Service account
        "accountName": "amsaccount",

        // [Required] The resource group of the Azure Media Service account
        "resourceGroup": "mediaservices-rg"
    }
Output:
    {
        // The created token
        "token": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1cm46bWljcm9zb2Z0OmF6dXJlOm1lZGlhc2VydmljZXM6Y29udGVudGtleWlkZW50aWZpZXIiOiJmZWQ1ZWE5Ni0wMjRhLTQ1YmMtYmE5My02MDFlZWU4MjcxNTgiLCJuYmYiOjE1ODc3MDUxOTgsImV4cCI6MTU4NzcwNTc5OCwiaXNzIjoibXlJc3N1ZXIiLCJhdWQiOiJteUF1ZGllbmNlIn0.UOLeEdiMfTKdIcr3SA-tO0nbD5AAedTKBAf815quNmI",
    }

```
*/
//
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using advanced_vod_functions_v3.SharedLibs;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace advanced_vod_functions_v3
{
    public static class CreateJwtToken
    {
        [FunctionName("CreateJwtToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"AMS v3 Function - CreateJwtToken was triggered!");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string streaminglocatorName = data.streamingLocatorName;
            string contentKeyPolicyName = data.contentKeyPolicyName;
            string accountName = data.accountName;
            string resourceGroup = data.resourceGroup;

            MediaServicesConfigWrapper amsconfig = new MediaServicesConfigWrapper();
            string token = null;

            try
            {
                IAzureMediaServicesClient client = MediaServicesHelper.CreateMediaServicesClientAsync(amsconfig);

                var response = client.StreamingLocators.ListContentKeys(resourceGroup, accountName, streaminglocatorName);
                var contentKeyId = response.ContentKeys.First().Id.ToString();
                var policyProperties = client.ContentKeyPolicies.GetPolicyPropertiesWithSecrets(resourceGroup, accountName, contentKeyPolicyName);

                var ckrestriction = (ContentKeyPolicyTokenRestriction)policyProperties.Options.FirstOrDefault()?.Restriction;
                var symKey = (ContentKeyPolicySymmetricTokenKey)ckrestriction.PrimaryVerificationKey;
                var tokenSigningKey = new SymmetricSecurityKey(symKey.KeyValue);

                SigningCredentials cred = new SigningCredentials(
                    tokenSigningKey,
                    // Use the  HmacSha256 and not the HmacSha256Signature option, or the token will not work!
                    SecurityAlgorithms.HmacSha256,
                    SecurityAlgorithms.Sha256Digest);

                Claim[] claims = new Claim[] {
                    new Claim(ContentKeyPolicyTokenClaim.ContentKeyIdentifierClaim.ClaimType, contentKeyId)
                };

                JwtSecurityToken jwtToken = new JwtSecurityToken(
                    issuer: ckrestriction.Issuer,
                    audience: ckrestriction.Audience,
                    claims: claims,
                    notBefore: DateTime.Now.AddMinutes(-5),
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: cred);

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                token = $"Bearer {handler.WriteToken(jwtToken)}";
            }
            catch (ApiErrorException e)
            {
                log.LogError($"ERROR: AMS API call failed with error code: {e.Body.Error.Code} and message: {e.Body.Error.Message}");
                return new BadRequestObjectResult("AMS API call error: " + e.Message + "\nError Code: " + e.Body.Error.Code + "\nMessage: " + e.Body.Error.Message);
            }
            catch (Exception e)
            {
                log.LogError($"ERROR: Exception with message: {e.Message}");
                return new BadRequestObjectResult("Error: " + e.Message);
            }

            return (ActionResult)new OkObjectResult(new
            {
                token = token
            });
        }
    }
}
