local.settings.json
===================

In order to be able to run and debug this application, you need to have a local.settings.json file in the same folder with this documentation file.

The contents of the file should be:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"

        // A CRON expression defining when to do full backups.
        "FullBackupSchedule": "0 0 0 */1 * *",

        // The name of the storage container to use for backups.
        "ContainerName": "cosmos-backups",
        
        // An array of connection strings serialized to a JSON string.
        "ConnectionStrings": "[\"string 1\", \"string 2\"]"
    },
    
    "CosmosBackup": {
        
        
        // An array of Cosmos DB Accounts to back up.
        "Accounts": [
            {
                // The connection string to the account to back up.
                "ConnectionString": string
            }
        ]
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

