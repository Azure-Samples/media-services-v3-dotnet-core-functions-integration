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

namespace LiveDrmOperationsV3.Helpers
{
    class CosmosHelpers
    {
        private DocumentClient _client;
        private ConfigWrapper _config;
        private ILogger _log;
        private bool init = false;
        private bool actionOutputs = false;
        private bool actionSettings = false;

        public CosmosHelpers(ILogger log, ConfigWrapper config)
        {
            if (string.IsNullOrEmpty(config.CosmosDB) || string.IsNullOrEmpty(config.CosmosConnectionString))
            {
                log.LogInformation("Cosmos access not configured.");
                return;
            }


            var dico = new Dictionary<string, string>();
            foreach (var s in config.CosmosConnectionString.Split(';'))
            {
                if (s != "")
                {
                    var pos = s.IndexOf('=');
                    dico.Add(s.Substring(0, pos), s.Substring(pos + 1));
                }
            }

            _client = new DocumentClient(new Uri(dico["AccountEndpoint"]), dico["AccountKey"]);
            _config = config;
            _log = log;
            init = true;

            if (!string.IsNullOrEmpty(_config.CosmosCollectionOutputs))
            {
                actionOutputs = true;
            }
            if (!string.IsNullOrEmpty(_config.CosmosCollectionSettings))
            {
                actionSettings = true;
            }
        }


        public async Task CreateOrUpdateGeneralInfoDocument(LiveEventEntry liveEvent)
        {
            if (!init || !actionOutputs) return;

            try
            {
                await this._client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_config.CosmosDB, _config.CosmosCollectionOutputs, liveEvent.Id), liveEvent);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this._client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_config.CosmosDB, _config.CosmosCollectionOutputs), liveEvent);
                }
                else
                {
                    throw;
                }
            }
        }


        public async Task DeleteGeneralInfoDocument(LiveEventEntry liveEvent)
        {
            if (!init || !actionOutputs) return;

            try
            {
                await this._client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_config.CosmosDB, _config.CosmosCollectionOutputs, liveEvent.Id));
            }
            catch (DocumentClientException de)
            {

            }
        }

        public async Task<LiveEventSettingsInfo> ReadSettingsDocument(string liveEventName)
        {
            if (!init || !actionSettings) return null;

            try
            {
                var result = await this._client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_config.CosmosDB, _config.CosmosCollectionSettings, liveEventName));
                return (dynamic)result.Resource;

            }
            catch (DocumentClientException de)
            {
                _log.LogWarning($"Settings for live event {liveEventName} not found in Cosmos db.");

                return null;
            }
        }

        public async Task CreateOrUpdateSettingsDocument(LiveEventSettingsInfo liveEvenSettings)
        {
            if (!init || !actionSettings) return;

            try
            {
                await this._client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_config.CosmosDB, _config.CosmosCollectionSettings, liveEvenSettings.Id), liveEvenSettings);
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this._client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_config.CosmosDB, _config.CosmosCollectionSettings), liveEvenSettings);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
