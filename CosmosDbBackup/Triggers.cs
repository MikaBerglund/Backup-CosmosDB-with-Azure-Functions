using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace CosmosDbBackup
{
    public static class Triggers
    {

        private const string FullBackupId = "805f7dee-d9d3-4e67-973c-2a977d9c50ac";

        [FunctionName(Names.FullBackupTimer)]
        public static async Task FullBackupTimer([TimerTrigger("%FullBackupSchedule%")]TimerInfo timer, [OrchestrationClient]DurableOrchestrationClient client, ILogger log)
        {
            await StartFullBackupMain(client, log);
        }

#if DEBUG
        [FunctionName(nameof(TriggerFullBackup))]
        public static async Task<HttpResponseMessage> TriggerFullBackup([HttpTrigger]HttpRequestMessage req, [OrchestrationClient]DurableOrchestrationClient client, ILogger log)
        {
            var status = await StartFullBackupMain(client, log);
            return req.CreateResponse(status ? HttpStatusCode.Accepted : HttpStatusCode.Conflict);
        }
#endif

        private static async Task<bool> StartFullBackupMain(DurableOrchestrationClient client, ILogger log)
        {
            var status = await client.GetStatusAsync(FullBackupId);
            if (
                null == status 
                || status.RuntimeStatus == OrchestrationRuntimeStatus.Canceled 
                || status.RuntimeStatus == OrchestrationRuntimeStatus.Completed 
                || status.RuntimeStatus == OrchestrationRuntimeStatus.Failed 
                || status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated
            )
            {
                await client.StartNewAsync(Names.FullBackupMain, FullBackupId, null);
                return true;
            }
            
            log.LogWarning($"A previous backup job is still in progress. Cannot start a new job. Current job - Status: '{status.RuntimeStatus}', instance ID: '{status.InstanceId}'.");
            return false;
        }
    }
}
