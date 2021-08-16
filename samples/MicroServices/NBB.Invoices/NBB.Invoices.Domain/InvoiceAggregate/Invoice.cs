// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using NBB.Domain;

namespace NBB.Invoices.Domain.InvoiceAggregate
{
    public class Invoice : EventedAggregateRoot<Guid>
    {
        public Guid InvoiceId { get; private set; }

        public Guid ClientId { get; private set; }

        public Guid? ContractId { get; private set; }

        public decimal Amount { get; private set; }

        public bool IsPayed => PaymentId.HasValue;

        public Guid? PaymentId { get; private set; }

        //needed 4 repository should be private
        public Invoice()
        {

        }

        public Invoice(Guid clientId, Guid? contractId, decimal amount)
        {
            InvoiceId = Guid.NewGuid();
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;

            AddEvent(new InvoiceCreated(InvoiceId, amount, clientId, contractId));
        }

        public override Guid GetIdentityValue() => InvoiceId;

        public void MarkAsPayed(Guid paymentId, Guid? contractId)
        {
            PaymentId = paymentId;
            AddEvent(new InvoicePayed(InvoiceId, paymentId, contractId));
        }
    }
}
