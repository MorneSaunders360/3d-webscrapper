using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebScrapperFunctionApp
{
    public class BulkUpsertResponse
    {
        public bool IsSuccessful => Errors.Count == 0;
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class ElasticsearchService<T> where T : class
    {
        private readonly ElasticClient _client;

        public ElasticsearchService( string defaultIndex)
        {
            var settings = new ConnectionSettings(new Uri("https://elastic.houselabs.co.za"))
                .ApiKeyAuthentication(new ApiKeyAuthenticationCredentials("QmRHMng1QUJiUTY3b25MMFZoNS06U09kR2ZkOWdTUHl2NGxPeEQwaEdYZw=="))
                .DefaultIndex(defaultIndex)
                .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
                .DisableDirectStreaming();

            _client = new ElasticClient(settings);
        }

        public async Task<IndexResponse> UpsertDocument(T document, Id id)
        {
            return await _client.IndexAsync(document, i => i.Id(id)).ConfigureAwait(false);
        }
        public async Task<BulkResponse> BulkUpsertDocuments(IEnumerable<T> documents, Func<T, string> idSelector)
        {
            var bulkRequest = new BulkDescriptor();
            foreach (var document in documents)
            {
                var id = idSelector(document);
                bulkRequest.Index<T>(op => op
                    .Document(document)
                    .Id(id)
                );
            }
            return await _client.BulkAsync(bulkRequest).ConfigureAwait(false);
        }
        public async Task<BulkUpsertResponse> BulkUpsertDocumentsInBatches(IEnumerable<T> documents, Func<T, string> idSelector, int batchSize = 1000)
        {
            var documentList = documents.ToList();
            int totalBatches = (documentList.Count + batchSize - 1) / batchSize;
            var bulkResponse = new BulkUpsertResponse();

            for (int i = 0; i < totalBatches; i++)
            {
                var batch = documentList.Skip(i * batchSize).Take(batchSize);

                var bulkRequest = new BulkDescriptor();
                foreach (var document in batch)
                {
                    var id = idSelector(document);
                    bulkRequest.Index<T>(op => op.Document(document).Id(id));
                }

                var response = await _client.BulkAsync(bulkRequest).ConfigureAwait(false);

                if (response.Errors)
                {
                    foreach (var item in response.ItemsWithErrors)
                    {
                        // Collect error details
                        bulkResponse.Errors.Add($"Error for document ID {item.Id}: {item.Error}");
                    }
                }
            }

            return bulkResponse;
        }



        public ISearchResponse<T> SearchDocuments(Func<SearchDescriptor<T>, ISearchRequest> selector)
        {
            return _client.Search<T>(selector);
        }
        public async Task<List<T>> SearchAllDocumentsAsync()
        {
            try
            {
                var searchResponse = await _client.SearchAsync<T>(s => s
                .MatchAll()
                .Size(1000) // Adjust the size based on your expected maximum number of documents
                );

                if (!searchResponse.IsValid || searchResponse.Documents == null)
                {
                    // Handle the error or return an empty list
                    return new List<T>();
                }

                return searchResponse.Documents.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public DeleteResponse DeleteDocument(DocumentPath<T> id)
        {
            return _client.Delete(id);
        }
    }
}
