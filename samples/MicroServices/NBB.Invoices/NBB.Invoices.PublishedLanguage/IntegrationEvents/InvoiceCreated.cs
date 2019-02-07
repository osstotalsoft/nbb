using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Invoices.PublishedLanguage.IntegrationEvents
{
    public class InvoiceCreated : Event
    {
        public Guid InvoiceId { get; private set; }
        public decimal Amount { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid? ContractId { get; private set; }


        [JsonConstructor]
        private InvoiceCreated(Guid eventId, ApplicationMetadata metadata,
            Guid invoiceId, decimal amount, Guid clientId, Guid? contractId)
            : base(eventId, metadata)
        {
            InvoiceId = invoiceId;
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }

        public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId)
            : this(Guid.NewGuid(), new ApplicationMetadata { CreationDate = DateTime.UtcNow }, 
                invoiceId, amount, clientId, contractId)
        {
        }
    }
}
