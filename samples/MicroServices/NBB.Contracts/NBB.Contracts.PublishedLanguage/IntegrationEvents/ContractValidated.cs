using NBB.Application.DataContracts;
using System;

namespace NBB.Contracts.PublishedLanguage.IntegrationEvents
{
    public class ContractValidated : Event
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }
        public decimal Amount { get; }

        public ContractValidated(Guid contractId, Guid clientId, decimal amount, EventMetadata metadata = null)
            : base(metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
            Amount = amount;
        }
    }
}
