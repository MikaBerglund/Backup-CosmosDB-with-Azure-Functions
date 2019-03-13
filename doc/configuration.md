Configuring the Application
===========================

Below are the configuration values you need to specify for the backup application.

Azure Functions Configuration
-----------------------------

The following settings are standard Azure Functions v2 configuration settings.

- `AzureWebJobsStorage`
- `FUNCTIONS_EXTENSION_VERSION`
- `FUNCTIONS_WORKER_RUNTIME`


Cosmos DB Backup Settings
-------------------------

The settings below are to configure the backup application.

- `CosmosBackup:FullBackupSchedule`: A [CRON](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer#cron-expressions) expression that specifies when to trigger a full backup. To run the backup every day at 3 AM, use the schedule `0 0 3 */1 * *`.
  
- `CosmosBackup:BackupStorage`: The storage account (connection string to it) to use for storage. If not specified, the storage account in `AzureWebJobsStorage` is used.
  
- `CosmosBackup:ContainerName`: The name of the container where to store the backups. If not specified, the default `cosmos-backup` is used.
  
- `CosmosBackup:DefaultConnectionString`: The default connection string to the Cosmos DB account to back up. If not specified, then the connection string must be specified for each account specified in the `CosmosBackup:Accounts` array described below.

The following settings are added for each database and / or collection you want to include in the backup. If none are specified, then all databases and all collections found by using the default connection string `CosmosBackup:DefaultConnectionString` are backed up.

- `CosmosBackup:Accounts:0:ConnectionString`: The connection string to the Cosmos DB account to back up. If not specified, `CosmosBackup:DefaultConnectionString` is used.
  
- `CosmosBackup:Accounts:0:DatabaseId`: The ID of the database to back up. If not specified, then all databases are backed up, and `CosmosBackup:Accounts:0:CollectionId` is ignored.
  
- `CosmosBackup:Accounts:0:CollectionId`: The ID of the collection to back up. If not specified, then all collections are backed up.


Disclaimer
----------

You probably know this, but it is always worth mentioning. You should not store your sensitive configuration data directly as configuration values on the Azure portal, but use services like [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/) that can help you protect your sensitive configuration, so that only your application can read it.

Might sound very hard, but it is actually pretty easy. [This 2-part instrunction article](https://microsoft.github.io/AzureTipsAndTricks/blog/tip180.html) is a good place to start.