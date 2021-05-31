using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LiveDrmOperationsV3.Helpers
{
    internal class IrdetoHelpers
    {
        public const string labelCenc = "cencDefaultKey";
        public const string labelCbcs = "cbcsDefaultKey";
        public const string assetprefix = "nb:cid:UUID:";

        private static async Task<HttpResponseMessage> CreateSoapEnvelopRegisterKeys(string url, string contentId,
            ConfigWrapper config, string keyId, string contentKey, string IV, bool fairPlay = false, ILogger log = null)
        {
            var soapString =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:liv=""http://man.entriq.net/livedrmservice/"">
<soapenv:Header>
<liv:LiveDrmServiceHeader m_sPassword = ""IrdetoPasswordToReplace"" m_sUsername = ""IrdetoUserNameToReplace"" KMSUsername = """" KMSPassword = """" />
</soapenv:Header>
<soapenv:Body>
<liv:RegisterKeys>
<liv:accountId>IrdetoAccountIdToReplace</liv:accountId>
<liv:contentId>contentidtoreplace</liv:contentId>
<liv:protectionSystem>
<toreplacewithdrmtech>
</liv:protectionSystem>
<liv:keys>
<liv:contentKey KeyId=""KeyIdToReplace"" ContentKey=""ContentKeyToReplace"" IV=""IVToReplace""/>
</liv:keys>
</liv:RegisterKeys>
</soapenv:Body>
</soapenv:Envelope>";


            var cencxml = @"<liv:string>Playready</liv:string>
<liv:string>Widevine</liv:string>";

            var fairplayxml = @"<liv:string>Streaming</liv:string>";

            soapString = fairPlay
                ? soapString.Replace("<toreplacewithdrmtech>", fairplayxml)
                : soapString.Replace("<toreplacewithdrmtech>", cencxml);
            soapString = soapString
                .Replace("contentidtoreplace", contentId)
                .Replace("IrdetoUserNameToReplace", config.IrdetoUserName)
                .Replace("IrdetoPasswordToReplace", config.IrdetoPassword)
                .Replace("IrdetoAccountIdToReplace", config.IrdetoAccountId)
                .Replace("KeyIdToReplace", keyId)
                .Replace("ContentKeyToReplace", contentKey)
                .Replace("IVToReplace", IV);

            if (log != null)
                log.LogInformation(soapString);

            var response = await PostXmlRequestRegisterKeys(url, soapString);
            return response;
        }

        private static async Task<string> CreateSoapEnvelopGenerateKeys(string url, string contentId,
            ConfigWrapper config, bool fairPlay = false)
        {
            var soapString =
                @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:liv=""http://man.entriq.net/livedrmservice/"">
<soapenv:Header>
<liv:LiveDrmServiceHeader m_sPassword = ""IrdetoPasswordToReplace"" m_sUsername = ""IrdetoUserNameToReplace"" KMSUsername = """" KMSPassword = """" />
</soapenv:Header>
<soapenv:Body>
<liv:GenerateKeys>
<liv:contentId>contentidtoreplace</liv:contentId>
<liv:contentId>contentidtoreplace</liv:contentId>
<liv:protectionSystem>
<toreplacewithdrmtech>
</liv:protectionSystem>
</liv:GenerateKeys>
</soapenv:Body>
</soapenv:Envelope>";

            var cencxml = @"<liv:string >Playready</liv:string>
<liv:string >Widevine</liv:string>";

            var fairplayxml = @"<liv:string >Streaming</liv:string>";

            soapString = fairPlay
                ? soapString.Replace("<toreplacewithdrmtech>", fairplayxml)
                : soapString.Replace("<toreplacewithdrmtech>", cencxml);
            soapString = soapString
                .Replace("contentidtoreplace", contentId).Replace("IrdetoUserNameToReplace", config.IrdetoUserName)
                .Replace("IrdetoAccountIdToReplace", config.IrdetoAccountId)
                .Replace("IrdetoPasswordToReplace", config.IrdetoPassword);

            var response = await PostXmlRequestGenerateKeys(url, soapString);
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        private static async Task<HttpResponseMessage> PostXmlRequestRegisterKeys(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://man.entriq.net/livedrmservice/RegisterKeys");

                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }

        private static async Task<HttpResponseMessage> PostXmlRequestGenerateKeys(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://man.entriq.net/livedrmservice/GenerateKeys");

                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }

        public static async Task<StreamingLocatorContentKey> GenerateAndRegisterCENCKeyToIrdeto(string contentId, ConfigWrapper config)
        {
            // CENC Key
            var cencGuid = Guid.NewGuid();
            string cenckeyId = cencGuid.ToString();
            string cenccontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
            var cencIV = Convert.ToBase64String(cencGuid.ToByteArray());
            var responsecenc = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService,
                contentId, config, cenckeyId, cenccontentKey, cencIV, false);
            var contentcenc = await responsecenc.Content.ReadAsStringAsync();

            if (responsecenc.StatusCode != HttpStatusCode.OK)
                throw new Exception("Error Irdeto response cenc - code " + responsecenc.StatusCode);
            //return IrdetoHelpers.ReturnErrorException(log, "Error Irdeto response cenc - " + contentcenc);


            return new StreamingLocatorContentKey()
            {
                Id = Guid.Parse(ReturnDataFromSoapResponse(contentcenc, "KeyId=")),
                Value = ReturnDataFromSoapResponse(contentcenc, "ContentKey=")
            };

        }

        public static async Task<StreamingLocatorContentKey> GenerateAndRegisterCBCSKeyToIrdeto(string liveEventName, ConfigWrapper config)
        {
            // CBCS Key
            var cbcsGuid = Guid.NewGuid();
            string cbcskeyId = cbcsGuid.ToString();
            string cbcscontentKey = Convert.ToBase64String(IrdetoHelpers.GetRandomBuffer(16));
            var cbcsIV =
                Convert.ToBase64String(
                    IrdetoHelpers.HexStringToByteArray(cbcsGuid.ToString().Replace("-", string.Empty)));
            var responsecbcs = await IrdetoHelpers.CreateSoapEnvelopRegisterKeys(config.IrdetoSoapService,
                liveEventName, config, cbcskeyId, cbcscontentKey, cbcsIV, true);
            var contentcbcs = await responsecbcs.Content.ReadAsStringAsync();

            if (responsecbcs.StatusCode != HttpStatusCode.OK)
                throw new Exception("Error Irdeto response cbcs - code " + responsecbcs.StatusCode);

            var cbcskeyIdFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "KeyId=");
            var cbcscontentKeyFromIrdeto = IrdetoHelpers.ReturnDataFromSoapResponse(contentcbcs, "ContentKey=");

            return new StreamingLocatorContentKey()
            {
                Id = Guid.Parse(ReturnDataFromSoapResponse(contentcbcs, "KeyId=")),
                Value = ReturnDataFromSoapResponse(contentcbcs, "ContentKey=")
            };

        }

        private static string ReturnDataFromSoapResponse(string xmlsoap, string item)
        {
            var position1 = xmlsoap.IndexOf(item, 0);
            var p1 = position1 + item.Length + 1;
            var position2 = xmlsoap.IndexOf(@"""", p1);

            return position1 > 0 ? xmlsoap.Substring(position1 + item.Length + 1, position2 - p1) : null;
        }


        public static byte[] GetRandomBuffer(int size)
        {
            var randomBytes = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            return randomBytes;
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public static IActionResult ReturnErrorException(ILogger log, Exception ex, string prefixMessage = null)
        {
            var message = "";
            if (ex.GetType() == typeof(ApiErrorException))
            {
                var exapi = (ApiErrorException)ex;
                if (exapi.Response != null)
                    message = exapi.Response.Content;
            }

            return ReturnErrorException(log,
                (prefixMessage == null ? string.Empty : prefixMessage + " : ") + ex.Message + message);
        }

        public static IActionResult ReturnErrorException(ILogger log, string message, string region = null)
        {
            MediaServicesHelpers.LogError(log, message, region);
            return new BadRequestObjectResult(
                new JObject
                {
                    {"success", false},
                    {"errorMessage", message},
                    {
                        "operationsVersion",
                        AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString()
                    }
                }.ToString());
        }

        public static async Task<StreamingPolicy> CreateStreamingPolicyIrdeto(ConfigWrapper config,
            IAzureMediaServicesClient client, string uniqueness)
        {
            var dash_smooth_protocol = new EnabledProtocols(false, true, false, true);
            var hls_dash_protocol = new EnabledProtocols(false, true, true, false);
            var cenc_config = new CencDrmConfiguration(
                new StreamingPolicyPlayReadyConfiguration
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoPlayReadyLAURL
                },
                new StreamingPolicyWidevineConfiguration
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoWidevineLAURL
                }
            );
            var cbcs_config = new CbcsDrmConfiguration(
                new StreamingPolicyFairPlayConfiguration
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoFairPlayLAURL
                }
            );

            var ContentKeysEnc = new StreamingPolicyContentKeys
            {
                DefaultKey = new DefaultKey
                {
                    Label = labelCenc
                }
            };

            var ContentKeysCbcsc = new StreamingPolicyContentKeys
            {
                DefaultKey = new DefaultKey
                {
                    Label = labelCbcs
                }
            };

            var cenc = new CommonEncryptionCenc(dash_smooth_protocol, null, ContentKeysEnc, cenc_config);
            var cbcs = new CommonEncryptionCbcs(hls_dash_protocol, null, ContentKeysCbcsc, cbcs_config);

            var policyName = "policy-" + uniqueness;
            var streamingPolicy = new StreamingPolicy(Guid.NewGuid().ToString(), policyName, null, DateTime.Now, null,
                null, cenc, cbcs, null);
            streamingPolicy = await client.StreamingPolicies.CreateAsync(config.ResourceGroup, config.AccountName,
                policyName, streamingPolicy);
            return streamingPolicy;
        }

        public static async Task<StreamingLocator> SetupDRMAndCreateLocatorWithNewKeys(ConfigWrapper config,
            string streamingPolicyName, string streamingLocatorName, IAzureMediaServicesClient client, Asset asset,
           StreamingLocatorContentKey cencKey, StreamingLocatorContentKey cbcsKey, Guid? streamingLocatorId,
           string alternativeMediaId = null, DateTime? startTime = null, DateTime? endTime = null)
        {

            cencKey.LabelReferenceInStreamingPolicy = labelCenc;
            cbcsKey.LabelReferenceInStreamingPolicy = labelCbcs;

            return await SetupDRMAndCreateLocatorWithExistingKeys(config, streamingPolicyName, streamingLocatorName, client, asset, cencKey, cbcsKey, streamingLocatorId, alternativeMediaId, startTime, endTime);
        }

        public static async Task<StreamingLocator> SetupDRMAndCreateLocatorWithExistingKeys(ConfigWrapper config,
           string streamingPolicyName, string streamingLocatorName, IAzureMediaServicesClient client, Asset asset,
          StreamingLocatorContentKey cencKey, StreamingLocatorContentKey cbcsKey, Guid? streamingLocatorId, string alternativeMediaId = null, DateTime? startTime = null, DateTime? endTime = null)
        {
            var locator = new StreamingLocator(
                asset.Name,
                streamingPolicyName,
                defaultContentKeyPolicyName: null,
                contentKeys: new List<StreamingLocatorContentKey> { cencKey, cbcsKey },
                streamingLocatorId: streamingLocatorId,
                startTime: startTime,
                endTime: endTime,
                alternativeMediaId: alternativeMediaId
            );

            locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName,
                streamingLocatorName, locator);

            return locator;
        }

        public static async Task<StreamingLocator> CreateClearLocator(ConfigWrapper config, string streamingLocatorName,
            IAzureMediaServicesClient client, Asset asset, Guid? streamingLocatorId, DateTime? startTime = null, DateTime? endTime = null)
        {
            var locator = new StreamingLocator(
                asset.Name,
                PredefinedStreamingPolicy.DownloadAndClearStreaming,
                defaultContentKeyPolicyName: null,
                contentKeys: null,
                streamingLocatorId: streamingLocatorId,
                startTime: startTime,
                endTime: endTime
            );

            locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName,
                streamingLocatorName, locator);
            return locator;
        }
    }
}