using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Invoices.PublishedLanguage.IntegrationQueries
{
    public class GetInvoice
    {
        public class Query : Query<Model>
        {
            public Guid InvoiceId { get; set; }

            [JsonConstructor]
            private Query(Guid queryId, ApplicationMetadata metadata, Guid invoiceId)
                : base(queryId, metadata)
            {
                InvoiceId = invoiceId;
            }

            public Query()
                : this(Guid.NewGuid(), new ApplicationMetadata { CreationDate = DateTime.UtcNow }, Guid.Empty)
            {

            }
        }

        public class Model
        {
            public Guid InvoiceId { get; set; }
            public Guid? ContractId { get; set; }
        }
    }
}
