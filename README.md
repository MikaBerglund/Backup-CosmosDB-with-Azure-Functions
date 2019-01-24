Backup Cosmos DB with Azure Functions
=====================================

This is a sample application that uses [Azure Functions](https://azure.microsoft.com/en-us/blog/introducing-azure-functions-2-0/) for backing up [Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) databases and collections to a blob storage container in an [Azure Storage](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview) account.


Overview
--------

The application is build on v2 of the Azure Functions runtime. It uses a [timer triggered](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer) function that starts an orchestration built using the [Durable Functions](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview) extension.

The orchestration function then takes care of reading all documents in the configured Cosmos DB database, and storing them in the configured storage account.


Disclaimer
----------

This application does NOT take a snapshot of the configured database, so the contents of the database may change while the database is being backed up. Therefore, the backup is not guaranteed to be in a consistent state when completed.

However, since there are no mechanisms for enforcing referential integrity, this is not that big of a problem as it would be for instance with a relational database like [SQL Database](https://azure.microsoft.com/en-us/services/sql-database/).