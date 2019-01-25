using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace CosmosDbBackup
{
    public class DocumentResponse
    {

        public string ContinuationToken { get; set; }

        public ICollection<Document> Documents { get; set; }

    }
}
