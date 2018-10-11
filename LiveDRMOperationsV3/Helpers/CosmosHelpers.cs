using LiveDrmOperationsV3;
using LiveDrmOperationsV3.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LiveDrmOperationsV3.Helpers

{
    class CosmosHelpers
    {
        private static IConfigurationRoot config = new ConfigurationBuilder()
                                                     .SetBasePath(Directory.GetCurrentDirectory())
                                                     .AddEnvironmentVariables()
                                                     .Build();

        private static string endpointUrl = config["CosmosDBAccountEndpoint"];
        private static string authorizationKey = config["CosmosDBAccountKey"];
        private static bool notInit = string.IsNullOrEmpty(endpointUrl) || string.IsNullOrEmpty(authorizationKey);
        private static DocumentClient _client = notInit ? null : new DocumentClient(new Uri(endpointUrl), authorizationKey);

        public static async Task<bool> CreateOrUpdateGeneralInfoDocument(LiveEventEntry liveEvent)  // true if success
        {
            if (notInit) return false;

            try
            {
                await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(config["CosmosDB"], config["CosmosCollectionOutputs"], liveEvent.Id), liveEvent);
                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(config["CosmosDB"], config["CosmosCollectionOutputs"]), liveEvent);
                    return true;
                }
                else
                {
                    throw;
                }
            }
        }


        public static async Task<bool> DeleteGeneralInfoDocument(LiveEventEntry liveEvent)
        {
            if (notInit) return false;

            try
            {
                await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(config["CosmosDB"], config["CosmosCollectionOutputs"], liveEvent.Id));
                return true;
            }
            catch (DocumentClientException de)
            {
                return false;
            }
        }

        public static async Task<LiveEventSettingsInfo> ReadSettingsDocument(string liveEventName)
        {
            if (notInit) return null;


            try
            {
                var result = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(config["CosmosDB"], config["CosmosCollectionSettings"], liveEventName));
                return (dynamic)result.Resource;

            }
            catch (DocumentClientException de)
            {

                return null;
            }
        }

        public static async Task<bool> CreateOrUpdateSettingsDocument(LiveEventSettingsInfo liveEvenSettings)
        {
            if (notInit) return false;

            try
            {
                await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(config["CosmosDB"], config["CosmosCollectionSettings"], liveEvenSettings.Id), liveEvenSettings);
                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(config["CosmosDB"], config["CosmosCollectionSettings"]), liveEvenSettings);
                    return true;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
