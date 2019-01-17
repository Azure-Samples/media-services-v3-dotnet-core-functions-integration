using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using LiveDrmOperationsV3.Models;
using LiveDRMOperationsV3.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace LiveDrmOperationsV3.Helpers
{
    internal partial class CosmosHelpers
    {
        private static readonly IConfigurationRoot Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .Build();

        private static readonly string EndpointUrl = Config["CosmosDBAccountEndpoint"];
        private static readonly string AuthorizationKey = Config["CosmosDBAccountKey"];
        private static readonly string Database = Config["CosmosDB"];

        private static readonly bool DoNotUsePartitionMode = !string.IsNullOrEmpty(Config["CosmosDoNotUsePartitionMode"]) && bool.Parse(Config["CosmosDoNotUsePartitionMode"]);
        private static readonly PartitionKeyDefinition PartitionKeyDef = new PartitionKeyDefinition { Paths = new Collection<string> { "/partitionKey" } };

        private static readonly int OfferThroughput = Config["CosmosOfferThroughput"] != null ? int.Parse(Config["CosmosOfferThroughput"]) : 400;

        private static readonly string CollectionOutputs = Config["CosmosCollectionLiveEventOutputInfo"];
        private static readonly string CollectionSettings = Config["CosmosCollectionLiveEventSettings"];


        private static readonly bool NotInit =
            string.IsNullOrEmpty(EndpointUrl) || string.IsNullOrEmpty(AuthorizationKey);

        private static Lazy<DocumentClient> lazyClient = new Lazy<DocumentClient>(InitializeDocumentClient);
        private static DocumentClient _client => lazyClient.Value;

        private static DocumentClient InitializeDocumentClient()
        {
            return NotInit ? null : new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
        }

        private static async Task<bool> CreateOrUpdateDocument(BaseModel myObject) // true if success
        {
            if (NotInit) return false;

            string id;
            string collectionId;

            if (myObject.GetType() == typeof(LiveEventEntry))
            {
                id = ((LiveEventEntry)myObject).Id;
                collectionId = CollectionOutputs;
            }
            else if (myObject.GetType() == typeof(LiveEventSettingsInfo))
            {
                id = ((LiveEventSettingsInfo)myObject).Id;
                collectionId = CollectionSettings;
            }
            else
            {
                return false;
            }

            try
            {
                await _client.ReplaceDocumentAsync(
                    UriFactory.CreateDocumentUri(Database, collectionId, id), myObject);
                return true;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            try // new document
            {
                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Database, collectionId), myObject);
                return true;

            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }

            try // let create the db, collection, and document
            {

                // we use shared RU for the database (RU are shared among the collections)
                await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = Database }, new RequestOptions { OfferThroughput = OfferThroughput });

                if (!DoNotUsePartitionMode)
                {
                    await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(Database), new DocumentCollection { Id = collectionId, PartitionKey = PartitionKeyDef });
                }
                else
                {
                    await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(Database), new DocumentCollection { Id = collectionId });
                }

                await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Database, collectionId), myObject);
                return true;
            }
            catch
            {

            }

            return false;
        }

        public static async Task<bool> CreateOrUpdateGeneralInfoDocument(LiveEventEntry liveEvent) // true if success
        {
            return await CreateOrUpdateDocument(liveEvent);
        }

        public static async Task<bool> DeleteGeneralInfoDocument(LiveEventEntry liveEvent)
        {
            if (NotInit) return false;

            try
            {
                await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionOutputs,
                    liveEvent.Id), DoNotUsePartitionMode ? null : new RequestOptions { PartitionKey = new PartitionKey(BaseModel.DefaultPartitionValue) });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<IQueryable<LiveEventEntry>> ReadGeneralInfoDocument(string liveEventName)
        {
            if (NotInit) return null;

            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, PartitionKey = DoNotUsePartitionMode ? null : new PartitionKey(BaseModel.DefaultPartitionValue) };

                var result = await Task.Run(() =>
                {
                    return _client.CreateDocumentQuery<LiveEventEntry>(
             UriFactory.CreateDocumentCollectionUri(Database, CollectionOutputs), queryOptions)
             .Where(f => f.LiveEventName == liveEventName);
                });
                return result;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<LiveEventSettingsInfo> ReadSettingsDocument(string liveEventName)
        {
            if (NotInit) return null;

            var liveSet = new LiveEventSettingsInfo() { LiveEventName = liveEventName };

            try
            {
                var result =
                    await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database, CollectionSettings,
                        liveSet.Id), DoNotUsePartitionMode ? null : new RequestOptions { PartitionKey = new PartitionKey(BaseModel.DefaultPartitionValue) });


                return (dynamic)result.Resource;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> CreateOrUpdateSettingsDocument(LiveEventSettingsInfo liveEvenSettings)
        {
            return await CreateOrUpdateDocument(liveEvenSettings);
        }
    }
}