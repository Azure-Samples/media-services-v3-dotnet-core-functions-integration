using Microsoft.Azure.Management.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;

namespace LiveDrmOperationsV3.Helpers
{
    class IrdetoHelpers
    {
        public const string labelCenc = "cencDefaultKey";
        public const string labelCbcs = "cbcsDefaultKey";
        public const string assetprefix = "nb:cid:UUID:";

        public static async Task<HttpResponseMessage> CreateSoapEnvelopRegisterKeys(string url, string contentId, ConfigWrapper config, string keyId, string contentKey, string IV, bool fairPlay = false, ILogger log = null)
        {
            string soapString = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:liv=""http://man.entriq.net/livedrmservice/"">
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


            string cencxml = @"<liv:string>Playready</liv:string>
<liv:string>Widevine</liv:string>";

            string fairplayxml = @"<liv:string>Streaming</liv:string>";

            soapString = fairPlay ? soapString.Replace("<toreplacewithdrmtech>", fairplayxml) : soapString.Replace("<toreplacewithdrmtech>", cencxml);
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

            HttpResponseMessage response = await PostXmlRequestRegisterKeys(url, soapString);
            return response;


        }

        public static async Task<string> CreateSoapEnvelopGenerateKeys(string url, string contentId, ConfigWrapper config, bool fairPlay = false)
        {
            string soapString = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:liv=""http://man.entriq.net/livedrmservice/"">
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

            string cencxml = @"<liv:string >Playready</liv:string>
<liv:string >Widevine</liv:string>";

            string fairplayxml = @"<liv:string >Streaming</liv:string>";

            soapString = fairPlay ? soapString.Replace("<toreplacewithdrmtech>", fairplayxml) : soapString.Replace("<toreplacewithdrmtech>", cencxml);
            soapString = soapString
                .Replace("contentidtoreplace", contentId).Replace("IrdetoUserNameToReplace", config.IrdetoUserName)
                .Replace("IrdetoAccountIdToReplace", config.IrdetoAccountId)
                .Replace("IrdetoPasswordToReplace", config.IrdetoPassword);

            HttpResponseMessage response = await PostXmlRequestGenerateKeys(url, soapString);
            string content = await response.Content.ReadAsStringAsync();

            return content;
        }

        public static async Task<HttpResponseMessage> PostXmlRequestRegisterKeys(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://man.entriq.net/livedrmservice/RegisterKeys");

                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }

        public static async Task<HttpResponseMessage> PostXmlRequestGenerateKeys(string baseUrl, string xmlString)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(xmlString, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://man.entriq.net/livedrmservice/GenerateKeys");

                return await httpClient.PostAsync(baseUrl, httpContent);
            }
        }

        public static string ReturnDataFromSoapResponse(string xmlsoap, string item)
        {
            var position1 = xmlsoap.IndexOf(item, 0);
            int p1 = position1 + item.Length + 1;
            var position2 = xmlsoap.IndexOf(@"""", p1);

            return position1 > 0 ? xmlsoap.Substring(position1 + item.Length + 1, position2 - p1) : null;
        }


        static public byte[] GetRandomBuffer(int size)
        {
            byte[] randomBytes = new byte[size];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
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
            string message = "";
            if (ex.GetType() == typeof(ApiErrorException))
            {
                var exapi = (ApiErrorException)ex;
                if (exapi.Response != null)
                    message = exapi.Response.Content;
            }
            return ReturnErrorException(log, ((prefixMessage == null) ? string.Empty : prefixMessage + " : ") + ex.Message + message);
        }

        public static IActionResult ReturnErrorException(ILogger log, string message)
        {
            log.LogError(message);
            return new BadRequestObjectResult(
                  (new JObject()
                                                        {
                                                                { "Success", false },
                                                                { "ErrorMessage", message },
                                                                { "OperationsVersion", AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString() }

                                                        }
                  ).ToString());
        }

        public static async Task<StreamingPolicy> CreateStreamingPolicyIrdeto(string contentId, ConfigWrapper config, IAzureMediaServicesClient client)
        {
            string uniqueness = Guid.NewGuid().ToString().Substring(0, 13);

            var dash_smooth_protocol = new EnabledProtocols(false, true, false, true);
            var hls_dash_protocol = new EnabledProtocols(false, true, true, false);
            var cenc_config = new CencDrmConfiguration(
                new StreamingPolicyPlayReadyConfiguration()
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoPlayReadyLAURL.Replace("{0}", contentId)
                },
                new StreamingPolicyWidevineConfiguration()
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoWidevineLAURL.Replace("{0}", contentId)
                }
                );
            var cbcs_config = new CbcsDrmConfiguration(
                new StreamingPolicyFairPlayConfiguration()
                {
                    CustomLicenseAcquisitionUrlTemplate = config.IrdetoFairPlayLAURL.Replace("{0}", contentId)
                }
                );

            var ContentKeysEnc = new StreamingPolicyContentKeys()
            {
                DefaultKey = new DefaultKey()
                {
                    Label = labelCenc// + contentId
                }
            };

            var ContentKeysCbcsc = new StreamingPolicyContentKeys()
            {
                DefaultKey = new DefaultKey()
                {
                    Label = labelCbcs// + contentId
                }
            };

            var cenc = new CommonEncryptionCenc(enabledProtocols: dash_smooth_protocol, clearTracks: null, contentKeys: ContentKeysEnc, drm: cenc_config);
            var cbcs = new CommonEncryptionCbcs(enabledProtocols: hls_dash_protocol, clearTracks: null, contentKeys: ContentKeysCbcsc, drm: cbcs_config);

            string policyname = contentId + "-" + uniqueness;
            var streamingPolicy = new StreamingPolicy(Guid.NewGuid().ToString(), policyname, null, DateTime.Now, null, null, cenc, cbcs, null);
            streamingPolicy = await client.StreamingPolicies.CreateAsync(config.ResourceGroup, config.AccountName, policyname, streamingPolicy);
            return streamingPolicy;
        }

        public static List<string> ReturnLocatorNameFromDescription(Asset liveAsset)
        {

            try
            {
                return (List<string>)JsonConvert.DeserializeObject(liveAsset.Description, typeof(List<string>));
            }

            catch
            {
                return new List<string>();
            }

            /*
            if (liveOutput.Description.Length > 9)
            {
                return liveOutput.Description.Substring(8); // for now we store the locator name in the live outpiut description
            }
            else
            {
                return null;
            }
            */
        }

        public static string SetLocatorNameInDescription(string locatorName, string existingDescription = null)
        {
            var mylist = new List<string>();
            if (!string.IsNullOrEmpty(existingDescription))
            {
                mylist = (List<string>)JsonConvert.DeserializeObject(existingDescription, typeof(List<string>));
            }
            mylist.Add(locatorName);
            return JsonConvert.SerializeObject(mylist); // for now we store the locator name in the live output description
        }


        public static async Task<StreamingLocator> SetupDRMAndCreateLocator(ConfigWrapper config, string streamingPolicyName, string streamingLocatorName, IAzureMediaServicesClient client, Asset asset, string cenckeyId, string cenccontentKey, string cbcskeyId, string cbcscontentKey)
        {
            StreamingLocatorContentKey keyCenc = new StreamingLocatorContentKey()
            {
                Id = Guid.Parse(cenckeyId),
                LabelReferenceInStreamingPolicy = labelCenc,// + contentId,
                Value = cenccontentKey
            };
            StreamingLocatorContentKey keyCbcs = new StreamingLocatorContentKey()
            {
                Id = Guid.Parse(cbcskeyId),
                LabelReferenceInStreamingPolicy = labelCbcs,// + contentId,
                Value = cbcscontentKey
            };

            StreamingLocator locator = new StreamingLocator(
                  assetName: asset.Name,
                  streamingPolicyName: streamingPolicyName,
                  defaultContentKeyPolicyName: null,
                  contentKeys: new List<StreamingLocatorContentKey>() { keyCenc, keyCbcs },
                  streamingLocatorId: null
                  );

            locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, streamingLocatorName, locator);
            return locator;
        }

        public static async Task<StreamingLocator> CreateClearLocator(ConfigWrapper config, string streamingLocatorName, IAzureMediaServicesClient client, Asset asset)
        {

            StreamingLocator locator = new StreamingLocator(
               assetName: asset.Name,
               streamingPolicyName: PredefinedStreamingPolicy.ClearStreamingOnly,
               defaultContentKeyPolicyName: null,
               contentKeys: null,
               streamingLocatorId: null
               );

            locator = await client.StreamingLocators.CreateAsync(config.ResourceGroup, config.AccountName, streamingLocatorName, locator);
            return locator;
        }
    }
}
