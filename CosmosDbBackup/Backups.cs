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
        public static async Task FullBackupMain([OrchestrationTrigger]DurableOrchestrationContext context, [OrchestrationClient]DurableOrchestrationClient client, ILogger log)
        {
            foreach(var cs in AppSettings.Current.ConnectionStrings)
            {
                var collections = await context.CallActivityAsync<IEnumerable<Uri>>(Names.EnumCollections, cs);

                foreach (var coll in collections)
                {
                    var jobDef = new CollectionBackupJob(context.CurrentUtcDateTime)
                    {
                        ConnectionString = cs,
                        CollectionLink = coll,
                        ContainerName = AppSettings.Current.ContainerName
                    };

                    await context.CallActivityAsync(Names.BackupDocuments, jobDef);
                }
            }
        }

    }
}
