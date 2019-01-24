local.settings.json
===================

In order to be able to run and debug this application, you need to have a local.settings.json file in the same folder with this documentation file.

The contents of the file should be:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",

        "ConnectionString": "<Your Cosmos DB connection string>",

        // A CRON expression defining when to do full backups.
        "FullBackupSchedule": "0 0 0 */1 * *",

        "ContainerName": "<The name of the container to store backups in.>"
    }
}
```

Azure Configuration
-------------------

When configuring your function application in the [Azure portal](https://portal.azure.com), you use the following configuration values.

- AzureWebJobsStorage
- FUNCTIONS_WORKER_RUNTIME
- ConnectionString
- FullBackupSchedule
- ContainerName

