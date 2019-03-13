using CosmosDbBackup.Configuration;
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
            var jobs = await context.CallActivityAsync<IEnumerable<FunctionParameters.CollectionBackupJob>>(Names.ResolveCollectionBackupJobs, null);

            foreach(var job in jobs)
            {
                try
                {
                    await context.CallActivityAsync(Names.BackupDocuments, job);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Error while backing up collection '{job.CollectionLink}'.");
                }
            }
        }

    }
}
