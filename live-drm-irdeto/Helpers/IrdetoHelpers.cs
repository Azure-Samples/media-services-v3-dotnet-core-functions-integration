using Microsoft.Azure.Management.Media;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Security.Cryptography;
using System.Linq;

namespace ImgDrmOperationsV2
{
    class IrdetoHelpers
    {
        public static async Task<HttpResponseMessage> CreateSoapEnvelopRegisterKeys(string url, string contentId, ConfigWrapper config, string keyId, string contentKey, string IV, bool fairPlay = false, TraceWriter log = null)
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
                log.Info(soapString);

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
    }
}
