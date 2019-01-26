using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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

        [FunctionName(Names.BackupDocuments)]
        public static async Task BackupDocuments([ActivityTrigger]DurableActivityContext context, ILogger log)
        {
            var jobDef = context.GetInput<CollectionBackupJob>();
            var client = await GetClientAsync(jobDef.ConnectionString);

            log.LogInformation($"Initializing backup for {client.ServiceEndpoint.Host} | {jobDef.CollectionLink}.");

            var dir = await GetDirectoryAsync(jobDef);

            using(var query = client.CreateDocumentQuery(jobDef.CollectionLink, new FeedOptions() { EnableCrossPartitionQuery = true }).AsDocumentQuery())
            {
                while(query.HasMoreResults)
                {
                    foreach(var doc in await query.ExecuteNextAsync<Document>())
                    {
                        var json = JsonConvert.SerializeObject(doc);

                        var docRef = dir.GetBlockBlobReference($"{doc.Id}.{doc.ResourceId}.json");
                        
                        await docRef.UploadTextAsync(json);

                        log.LogMetric("Document.BackedUp", 1);
                        log.LogInformation($"Backed up '{docRef.StorageUri.PrimaryUri}'");
                    }
                }
            }
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

        private static async Task<CloudBlobDirectory> GetDirectoryAsync(CollectionBackupJob jobDef)
        {
            var client = await GetClientAsync(jobDef.ConnectionString);
            var storage = CloudStorageAccount.Parse(AppSettings.Current.AzureWebJobsStorage);
            var blobs = storage.CreateCloudBlobClient();
            var container = blobs.GetContainerReference(jobDef.ContainerName.ToLower());
            await container.CreateIfNotExistsAsync();

            // The collection URI is always 'dbs/[name of db]/colls/[name of collection]' so the
            // db is item 1 and collection is item 3 when splitting by '/'.
            var arr = jobDef.CollectionLink.OriginalString.Split('/');

            CloudBlobDirectory dir = container
                .GetDirectoryReference(client.ServiceEndpoint.Host)
                .GetDirectoryReference(arr[1])
                .GetDirectoryReference(arr[3])
                .GetDirectoryReference(jobDef.Timestamp.ToString("yyyyMMdd-HHmmss"))
                ;

            return dir;
        }

        private static async Task StoreDocumentAsync(CloudBlobContainer container, Document doc)
        {

        }

    }
}
