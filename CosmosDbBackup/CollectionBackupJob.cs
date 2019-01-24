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

        /// <summary>
        /// The connection string to the Cosmos DB account to back up.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The collection link to the collection to back up.
        /// </summary>
        public Uri CollectionLink { get; set; }

    }
}
