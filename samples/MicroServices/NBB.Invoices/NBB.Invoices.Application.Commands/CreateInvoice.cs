using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using System;

namespace NBB.Invoices.Application.Commands
{
    public class CreateInvoice : Command, IKeyProvider
    {
        public decimal Amount { get; }

        public Guid ClientId { get; }

        public Guid? ContractId { get; }

        string IKeyProvider.Key => ContractId.ToString();

        public CreateInvoice(decimal amount, Guid clientId, Guid? contractId, CommandMetadata metadata = null)
            : base(metadata)
        {
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }
    }
}
