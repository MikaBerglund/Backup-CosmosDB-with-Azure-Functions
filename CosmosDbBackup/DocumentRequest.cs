using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup
{
    public class DocumentRequest
    {

        public string ConnectionString { get; set; }

        public Uri CollectionLink { get; set; }

        public string ContinuationToken { get; set; }

    }
}
