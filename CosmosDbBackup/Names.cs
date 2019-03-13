using System;
using CosmosDbBackup.Configuration;
using CosmosDbBackup.FunctionParameters;

namespace CosmosDbBackup
{
    public static class Names
    {

        /// <summary>
        /// The name of the timer triggered function that kicks of a new full backup orchestration.
        /// </summary>
        public const string FullBackupTimer = nameof(FullBackupTimer);

        /// <summary>
        /// The HTTP triggered function that triggers a full backup when invoked.
        /// </summary>
        public const string FullBackupHttpTrigger = nameof(FullBackupHttpTrigger);

        /// <summary>
        /// The orchestration function that takes care of backing up the configured databases.
        /// </summary>
        public const string FullBackupMain = nameof(FullBackupMain);

        /// <summary>
        /// The activity function that takes care of backing up all documents in a collection. The input must be an instance of type <see cref="CollectionBackupJob"/>.
        /// </summary>
        public const string BackupDocuments = nameof(BackupDocuments);

        /// <summary>
        /// The activity function that from the current settings, resolves all backup jobs that need to be executed. The function returns an array of <see cref="CollectionBackupJob"/> instances. The function does not take an input.
        /// </summary>
        public const string ResolveCollectionBackupJobs = nameof(ResolveCollectionBackupJobs);

    }
}
