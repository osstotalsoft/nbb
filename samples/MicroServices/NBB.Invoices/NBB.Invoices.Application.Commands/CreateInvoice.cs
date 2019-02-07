using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using Newtonsoft.Json;
using System;

namespace NBB.Invoices.Application.Commands
{
    public class CreateInvoice : Command, IKeyProvider
    {
        public decimal Amount { get; }

        public Guid ClientId { get; }

        public Guid? ContractId { get; }

        string IKeyProvider.Key => ContractId.ToString();

        [JsonConstructor]
        private CreateInvoice(Guid commandId, ApplicationMetadata metadata, decimal amount, Guid clientId, Guid? contractId)
            : base(commandId, metadata)
        {
            Amount = amount;
            ClientId = clientId;
            ContractId = contractId;
        }

        public CreateInvoice(decimal amount, Guid clientId, Guid? contractId, Guid? correlationId)
            : this(Guid.NewGuid(),
                new ApplicationMetadata { CreationDate = DateTime.UtcNow },
                amount, clientId, contractId)
        {
        }
    }
}
