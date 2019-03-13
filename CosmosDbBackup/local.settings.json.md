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
    },
    
    "CosmosBackup": {
        "FullBackupSchedule": "0 0 0 */1 * *",
        "BackupStorage": <string>,
        "ContainerName": "cosmos-backups",
        "DefaultConnectionString": <string>,
        "Accounts": [
            {
                "ConnectionString": <string>,
                "DatabaseId": <string>,
                "CollectionId": <string>
            }
        ]
    }
}
```

See [configuration](../doc/configuration.md) for more information on these configuration values.

Azure Configuration
-------------------

When you [deploy](../doc/deployment.md) the application to Azure, you don't store the application configuration in a JSON file. Instead, you specify the configuration values for the application on the Azure portal. All necessary configuration is detailed in [configuration](../doc/configuration.md).
