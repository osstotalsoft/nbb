using NBB.Application.DataContracts;
using System;

namespace NBB.Invoices.PublishedLanguage.IntegrationEvents
{
    public class InvoiceCreated : Event
    {
        public Guid InvoiceId { get; }
        public decimal Amount { get; }
        public Guid ClientId { get; }
        public Guid? ContractId { get; }

        public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId, EventMetadata metadata = null)
            : base(metadata)
        {
            InvoiceId = invoiceId;
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }
    }
}
