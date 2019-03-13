using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup.Configuration
{
    /// <summary>
    /// Provides typed access to the configuration variables.
    /// </summary>
    public class AppSettings
    {

        private AppSettings()
        {

            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot"))
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();


            this.AzureWebJobsStorage = config.GetValue<string>("AzureWebJobsStorage");
            this.CosmosBackup = config.GetSection("CosmosBackup").Get<CosmosBackupSettings>();

            if(null != this.CosmosBackup)
            {
                if (null != this.CosmosBackup.Accounts)
                {
                    foreach (var acc in this.CosmosBackup.Accounts)
                    {
                        if (string.IsNullOrWhiteSpace(acc.ConnectionString)) acc.ConnectionString = this.CosmosBackup.DefaultConnectionString;
                    }
                }

                if (string.IsNullOrWhiteSpace(this.CosmosBackup.BackupStorage)) this.CosmosBackup.BackupStorage = this.AzureWebJobsStorage;
                if (string.IsNullOrWhiteSpace(this.CosmosBackup.ContainerName)) this.CosmosBackup.ContainerName = "cosmos-backup";
            }
        }

        public static AppSettings Current { get; private set; } = new AppSettings();



        public string AzureWebJobsStorage { get; private set; }

        public CosmosBackupSettings CosmosBackup { get; private set; }


        private static IConfiguration CreateConfiguration(ExecutionContext context = null)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            if(null != context)
            {
                builder
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", true, true);
            }

            builder.AddEnvironmentVariables();

            return builder.Build();
        }
    }

}
