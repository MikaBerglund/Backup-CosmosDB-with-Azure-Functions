Backup Cosmos DB with Azure Functions
=====================================

This is a sample application that uses [Azure Functions](https://azure.microsoft.com/en-us/blog/introducing-azure-functions-2-0/) for backing up [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) databases and collections to a blob storage container in an [Azure Storage](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview) account.


Overview
--------

The application is build on v2 of the Azure Functions runtime. It uses a [timer triggered](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer) function that starts an orchestration built using the [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview) extension.

The orchestration function then takes care of reading all documents in the configured Cosmos DB database, and storing them in the configured storage account.

> Note that *Azure Cosmos DB* automatically backs up all your data, and keeps the backups available in case you need to restore. However, to restore a Cosmos DB database from backup, you must [file a support ticket or call Azure support](https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-backup-and-restore) to ask them have the data restored.

To be able to better control how my backups are done, and how I restore data in case I need to, I wrote this application and thought you might want to use it too, or fork it to modify it to better suit your needs.


Contribute
----------

If you think there's something missing or want to add functionality, please consider sending me a pull request with your improvements.


Configuration
-------------

Read details about how to [configure](doc/configure.md) the application before deploying and running it.


Disclaimer
----------

This application does NOT take a snapshot of the configured database, so the contents of the database may change while the database is being backed up. Therefore, the backup is not guaranteed to be in a consistent state when completed.

However, since there are no mechanisms for enforcing referential integrity, this is not that big of a problem as it would be for instance with a relational database like [SQL Database](https://azure.microsoft.com/en-us/services/sql-database/).