using LiveDrmOperationsV3.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LiveDrmOperationsV3.Helpers
{
    class CosmosHelpers
    {
        private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
                                                     .SetBasePath(Directory.GetCurrentDirectory())
                                                     .AddEnvironmentVariables()
                                                     .Build();

        private static readonly string EndpointUrl = Config["CosmosDBAccountEndpoint"];
        private static readonly string AuthorizationKey = Config["CosmosDBAccountKey"];
        private static readonly string Database = Config["CosmosDB"];

        private static readonly string CollectionOutputs = Config["CosmosCollectionOutputs"];
        private static readonly string CollectionSettings = Config["CosmosCollectionSettings"];

        private static readonly bool NotInit = string.IsNullOrEmpty(EndpointUrl) || string.IsNullOrEmpty(AuthorizationKey);
        private static DocumentClient _client = NotInit ? null : new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);

        public static async Task<bool> CreateOrUpdateGeneralInfoDocument(LiveEventEntry liveEvent)  // true if success
        {
            if (NotInit) return false;

            try
            {
                await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionOutputs, liveEvent.Id), liveEvent);
                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Database, CollectionOutputs), liveEvent);
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
            if (NotInit) return false;

            try
            {
                await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionOutputs, liveEvent.Id));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<LiveEventSettingsInfo> ReadSettingsDocument(string liveEventName)
        {
            if (NotInit) return null;

            try
            {
                var result = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionSettings, liveEventName));
                return (dynamic)result.Resource;

            }
            catch
            {

                return null;
            }
        }

        public static async Task<bool> CreateOrUpdateSettingsDocument(LiveEventSettingsInfo liveEvenSettings)
        {
            if (NotInit) return false;

            try
            {
                await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionSettings, liveEvenSettings.Id), liveEvenSettings);
                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Database, CollectionSettings), liveEvenSettings);
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