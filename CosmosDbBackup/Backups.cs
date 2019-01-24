using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDbBackup
{
    public static class Backups
    {


        [FunctionName(Names.FullBackupMain)]
        public static async Task FullBackupMain([OrchestrationTrigger]DurableOrchestrationContext context, ILogger log)
        {
            var collections = await context.CallActivityAsync<IEnumerable<Uri>>(Names.EnumCollections, AppSettings.Current.ConnectionString);

            foreach(var coll in collections)
            {
                var jobDef = new CollectionBackupJob(context.CurrentUtcDateTime)
                {
                    ConnectionString = AppSettings.Current.ConnectionString,
                    CollectionLink = coll,
                    ContainerName = AppSettings.Current.ContainerName
                };

                await context.CallSubOrchestratorAsync(Names.BackupCollection, jobDef);
            }
        }

    }
}
