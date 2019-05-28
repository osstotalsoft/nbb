using NBB.Domain;
using System;

namespace NBB.Invoices.Domain.InvoiceAggregate.DomainEvents
{
    public class InvoiceCreated : DomainEvent
    {
        public Guid InvoiceId { get; }

        public decimal Amount { get; }

        public Guid ClientId { get; }

        public Guid? ContractId { get; }

        public InvoiceCreated(Guid invoiceId, decimal amount, Guid clientId, Guid? contractId, DomainEventMetadata metadata = null) 
            : base(metadata)
        {
            InvoiceId = invoiceId;
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }

    }
}
