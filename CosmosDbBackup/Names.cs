﻿using System;
using System.Collections.Generic;
using System.Text;

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

    }
}