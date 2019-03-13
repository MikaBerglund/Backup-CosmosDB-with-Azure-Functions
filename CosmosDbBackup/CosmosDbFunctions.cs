using CosmosDbBackup.Configuration;
using CosmosDbBackup.FunctionParameters;
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

        [FunctionName(Names.BackupDocuments)]
        public static async Task BackupDocuments([ActivityTrigger]DurableActivityContext context, ILogger log)
        {
            var jobDef = context.GetInput<CollectionBackupJob>();
            var client = await GetClientAsync(jobDef.ConnectionString);

            var name = $"{client.ServiceEndpoint.Host} | {jobDef.CollectionLink}";

            log.LogInformation($"Initializing backup for {name}.");

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
                    }
                }
            }

            log.LogInformation($"Completed backup for {name}.");
        }

        [FunctionName(Names.ResolveCollectionBackupJobs)]
        public static async Task<IEnumerable<CollectionBackupJob>> ResolveCollectionBackupJobs([ActivityTrigger]DurableActivityContext context, ILogger logger)
        {
            var jobs = new List<CollectionBackupJob>();

            if(null != AppSettings.Current?.CosmosBackup?.Accounts)
            {
                foreach(var acc in AppSettings.Current.CosmosBackup.Accounts)
                {
                    IEnumerable<string> dbs = null;
                    if(!string.IsNullOrWhiteSpace(acc.DatabaseId))
                    {
                        dbs = new string[] { acc.DatabaseId };
                    }
                    else
                    {
                        dbs = await EnumDatabasesAsync(acc.ConnectionString);
                    }

                    foreach (var db in dbs)
                    {
                        IEnumerable<string> colls = null;

                        if(string.IsNullOrWhiteSpace(acc.DatabaseId) || string.IsNullOrWhiteSpace(acc.CollectionId))
                        {
                            // If database has not been specified (in which case we ignore the collection) or if collection has not been specified, then we enumerate all collection in the current database.
                            colls = await EnumCollectionsAsync(acc.ConnectionString, db);
                        }
                        else
                        {
                            // Both database and collection is specified, so we connect only to the specified collection.
                            colls = new string[] { acc.CollectionId };
                        }

                        foreach(var coll in colls)
                        {
                            jobs.Add(new CollectionBackupJob(DateTime.UtcNow)
                            {
                                ConnectionString = acc.ConnectionString,
                                CollectionLink = UriFactory.CreateDocumentCollectionUri(db, coll),
                                ContainerName = AppSettings.Current.CosmosBackup.ContainerName,
                                Storage = AppSettings.Current.CosmosBackup.BackupStorage
                            });
                        }
                    }
                }
            }
            else if(!string.IsNullOrWhiteSpace(AppSettings.Current?.CosmosBackup?.DefaultConnectionString))
            {
                // No acounts have been specified, so we process all databases and all collections in default account.
                var dbs = await EnumDatabasesAsync(AppSettings.Current.CosmosBackup.DefaultConnectionString);
                foreach(var db in dbs)
                {

                }
            }
            else
            {
                logger.LogError("The application is not properly configured. Cannot resolve collections to back up. Please check the configuration for the application.");
            }

            return jobs;
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

        /// <summary>
        /// Creates a new client object from the given connection string.
        /// </summary>
        /// <remarks>
        /// Do not call this method directly, but use the <see cref="GetClientAsync(string)"/> method instead. That method caches the client objects to prevent the application from running out of HTTP connections. Not actually sure if this is even necessary...
        /// </remarks>
        private static DocumentClient CreateNewClient(string connectionString)
        {
            var builder = new DbConnectionStringBuilder() { ConnectionString = connectionString };
            var endpoint = (string)builder["AccountEndpoint"];
            var key = (string)builder["AccountKey"];

            var client = new DocumentClient(new Uri(endpoint), key);
            return client;
        }

        /// <summary>
        /// Connects to the account specified in <paramref name="connectionString"/> and database specified in <paramref name="database"/> and returns an array of all collections in that database.
        /// </summary>
        private static async Task<IEnumerable<string>> EnumCollectionsAsync(string connectionString, string database)
        {
            var list = new List<string>();

            if(!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = await GetClientAsync(connectionString);
                using (var query = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(database)).AsDocumentQuery())
                {
                    while(query.HasMoreResults)
                    {
                        foreach(var c in await query.ExecuteNextAsync<DocumentCollection>())
                        {
                            list.Add(c.Id);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Connects to the account specified in <paramref name="connectionString"/> and returns an array of database names.
        /// </summary>
        private async static Task<IEnumerable<string>> EnumDatabasesAsync(string connectionString)
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var client = await GetClientAsync(connectionString);

                using (var dbQuery = client.CreateDatabaseQuery().AsDocumentQuery())
                {
                    while(dbQuery.HasMoreResults)
                    {
                        foreach(var db in await dbQuery.ExecuteNextAsync<Database>())
                        {
                            list.Add(db.Id);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Returns the directory object where to store documents for the given backup job.
        /// </summary>
        private static async Task<CloudBlobDirectory> GetDirectoryAsync(CollectionBackupJob jobDef)
        {
            var client = await GetClientAsync(jobDef.ConnectionString);
            var storage = CloudStorageAccount.Parse(jobDef.Storage);
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

    }
}
