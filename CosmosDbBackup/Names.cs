using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbBackup
{
    public static class Names
    {

        /// <summary>
        /// The name of the timer triggered function that kicks of a new full backup orchestration.
        /// </summary>
        public const string FullBackupTimer = nameof(FullBackupTimer);

        /// <summary>
        /// The orchestration function that takes care of backing up the configured databases.
        /// </summary>
        public const string FullBackupMain = nameof(FullBackupMain);

        /// <summary>
        /// The activity function that enumerates the databases and collections in the configured Cosmos DB account. The result is returned as
        /// a collection of <see cref="Uri"/> objects, where each item represents the document collection URI to the collection to back up. The input to the
        /// function must be the connection string to the Cosmos DB account to connect to.
        /// </summary>
        public const string EnumCollections = nameof(EnumCollections);

        /// <summary>
        /// The orchestration function that takes care of backing up the given Cosmos DB collection. The input must be an instance of the <see cref="CollectionBackupJob"/> type.
        /// </summary>
        public const string BackupCollection = nameof(BackupCollection);

        /// <summary>
        /// The activity function that returns documents from a document collection. The input must be a <see cref="DocumentRequest"/> type, and 
        /// the function returns an instance of the <see cref="DocumentResponse"/>
        /// </summary>
        public const string GetDocumentsToBackUp = nameof(GetDocumentsToBackUp);

    }
}
