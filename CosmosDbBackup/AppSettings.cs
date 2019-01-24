using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup
{
    /// <summary>
    /// Provides typed access to the configuration variables.
    /// </summary>
    public static class AppSettings
    {

        public static string AzureWebJobsStorage
        {
            get { return Environment.GetEnvironmentVariable("AzureWebJobsStorage"); }
        }

        public static string ConnectionString
        {
            get { return Environment.GetEnvironmentVariable("ConnectionString"); }
        }

        public static string ContainerName
        {
            get { return Environment.GetEnvironmentVariable("ContainerName"); }
        }

    }
}
