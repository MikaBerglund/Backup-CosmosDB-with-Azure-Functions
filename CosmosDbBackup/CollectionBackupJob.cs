using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup
{
    /// <summary>
    /// A class that contains information about a collection backup job.
    /// </summary>
    public class CollectionBackupJob
    {

        public CollectionBackupJob(DateTime timestamp)
        {
            this.Timestamp = timestamp;
        }

        /// <summary>
        /// The connection string to the Cosmos DB account to back up.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The collection link to the collection to back up.
        /// </summary>
        public Uri CollectionLink { get; set; }

        /// <summary>
        /// The name of the container to store the documents in.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// A timestamp when the object was created. Used to identify the backup set.
        /// </summary>
        public DateTime Timestamp { get; private set; }
    }
}
