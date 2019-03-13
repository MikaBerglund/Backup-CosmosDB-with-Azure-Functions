using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup.Configuration
{

    public class CosmosBackupSettings
    {
        public CosmosBackupSettings()
        {
            this.Accounts = new List<CosmosAccountSettings>();
        }



        public string FullBackupSchedule { get; set; }

        public string BackupStorage { get; set; }

        public string ContainerName { get; set; }

        public string DefaultConnectionString { get; set; }

        public ICollection<CosmosAccountSettings> Accounts { get; set; }
    }

    public class CosmosAccountSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseId { get; set; }

        public string CollectionId { get; set; }

    }
}
