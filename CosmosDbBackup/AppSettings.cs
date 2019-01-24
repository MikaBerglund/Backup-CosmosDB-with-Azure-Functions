using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup
{
    /// <summary>
    /// Provides typed access to the configuration variables.
    /// </summary>
    public class AppSettings
    {

        private AppSettings()
        {
            this.AzureWebJobsStorage = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            this.ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
            this.ContainerName = Environment.GetEnvironmentVariable("ContainerName");
        }

        public static AppSettings Current { get; } = new AppSettings();


        public string AzureWebJobsStorage { get; private set; }

        public string ConnectionString { get; private set; }

        public string ContainerName { get; private set; }

    }
}
