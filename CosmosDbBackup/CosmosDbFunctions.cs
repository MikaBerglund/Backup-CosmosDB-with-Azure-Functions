using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosDbBackup
{
    public static class CosmosDbFunctions
    {

        [FunctionName(Names.EnumCollections)]
        public static async Task<IEnumerable<Uri>> EnumCollections([ActivityTrigger]DurableActivityContext context, ILogger log)
        {
            var connectionString = context.GetInput<string>();

            var list = new List<Uri>();

            var client = await GetClientAsync(connectionString);

            using (var dbQuery = client.CreateDatabaseQuery().AsDocumentQuery())
            {
                while (dbQuery.HasMoreResults)
                {
                    foreach (var db in await dbQuery.ExecuteNextAsync<Database>())
                    {
                        using (var collQuery = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(db.Id)).AsDocumentQuery())
                        {
                            while (collQuery.HasMoreResults)
                            {
                                foreach (var coll in await collQuery.ExecuteNextAsync<DocumentCollection>())
                                {
                                    list.Add(UriFactory.CreateDocumentCollectionUri(db.Id, coll.Id));
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        [FunctionName(Names.BackupCollection)]
        public static async Task BackupCollection([OrchestrationTrigger]DurableOrchestrationContext context, ILogger log)
        {
            var jobDef = context.GetInput<CollectionBackupJob>();
            var client = await GetClientAsync(jobDef.ConnectionString);
            
            log.LogInformation($"Initializing backup for {client.ServiceEndpoint.Host} | {jobDef.CollectionLink}.");

            var request = new DocumentRequest()
            {
                CollectionLink = jobDef.CollectionLink,
                ConnectionString = jobDef.ConnectionString
            };

            do
            {
                try
                {
                    var response = await context.CallActivityAsync<DocumentResponse>(Names.GetDocumentsToBackUp, request);
                    request.ContinuationToken = response.ContinuationToken;
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                }
            }
            while (null != request.ContinuationToken);
        }

        [FunctionName(Names.GetDocumentsToBackUp)]
        public static async Task<DocumentResponse> GetDocumentsToBackUp([ActivityTrigger]DurableActivityContext context, ILogger log)
        {
            var request = context.GetInput<DocumentRequest>();

            var client = await GetClientAsync(request.ConnectionString);
            DocumentResponse response = null;

            using (
                var query = client.CreateDocumentQuery(
                    request.CollectionLink, 
                    new FeedOptions() { EnableCrossPartitionQuery = true, RequestContinuation = request.ContinuationToken }
                ).AsDocumentQuery()
            )
            {
                var result = await query.ExecuteNextAsync<Document>();
                response = new DocumentResponse()
                {
                    ContinuationToken = result.ResponseContinuation,
                    Documents = result.ToList()
                };
            }

            return response;
        }

        /// <summary>
        /// Executes a document query and returns all results for that query. Use with caution where the result set can potentially be very large.
        /// </summary>
        private static async Task<IEnumerable<T>> ExecuteAllAsync<T>(IDocumentQuery<T> query)
        {
            var list = new List<T>();

            while (query.HasMoreResults)
            {
                list.AddRange(await query.ExecuteNextAsync<T>());
            }
            
            return list;
        }

        private static SemaphoreSlim clientLock = new SemaphoreSlim(1, 1);
        private static Dictionary<string, DocumentClient> clients = new Dictionary<string, DocumentClient>();
        /// <summary>
        /// Returns a <see cref="DocumentClient"/> object for the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>This method returns an existing instance, or creates a new, if an instnace for the given connection string was nout found.</returns>
        private static async Task<DocumentClient> GetClientAsync(string connectionString)
        {
            await clientLock.WaitAsync();
            try
            {
                if (!clients.ContainsKey(connectionString))
                {
                    clients[connectionString] = CreateNewClient(connectionString);
                }
            }
            finally
            {
                clientLock.Release();
            }

            return clients[connectionString];
        }

        private static DocumentClient CreateNewClient(string connectionString)
        {
            var builder = new DbConnectionStringBuilder() { ConnectionString = connectionString };
            var endpoint = (string)builder["AccountEndpoint"];
            var key = (string)builder["AccountKey"];

            var client = new DocumentClient(new Uri(endpoint), key);
            return client;
        }

    }
}
