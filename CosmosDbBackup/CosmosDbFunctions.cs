using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

            var dbQuery = client.CreateDatabaseQuery().AsDocumentQuery();
            var databases = await ExecuteAllAsync(dbQuery);
            
            foreach(var db in databases)
            {
                var collQuery = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(db.Id)).AsDocumentQuery();
                var collections = await ExecuteAllAsync(collQuery);
                foreach(var coll in collections)
                {
                    list.Add(UriFactory.CreateDocumentCollectionUri(db.Id, coll.Id));
                }
            }

            return list;
        }



        /// <summary>
        /// Executes a document query and returns all results for that query. Use with caution where the result set can potentially be very large.
        /// </summary>
        private static async Task<IEnumerable<T>> ExecuteAllAsync<T>(IDocumentQuery<T> query)
        {
            var list = new List<T>();

            string token = null;
            do
            {
                var response = await query.ExecuteNextAsync<T>();
                if(null != response)
                {
                    list.AddRange(response);
                }
                token = response?.ResponseContinuation;
            }
            while (null != token);
            
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
                if(!clients.ContainsKey(connectionString))
                {
                    var builder = new DbConnectionStringBuilder() { ConnectionString = connectionString };
                    var endpoint = (string)builder["AccountEndpoint"];
                    var key = (string)builder["AccountKey"];

                    var client = new DocumentClient(new Uri(endpoint), key);
                    clients[connectionString] = client;
                }
            }
            finally
            {
                clientLock.Release();
            }

            return clients[connectionString];
        }

    }
}
