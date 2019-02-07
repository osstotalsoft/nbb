using NBB.Application.DataContracts;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.PublishedLanguage.IntegrationEvents
{
    public class ContractValidated : Event
    {
        public Guid ContractId { get; }
        public Guid ClientId { get; }
        public decimal Amount { get; }

        [JsonConstructor]
        private ContractValidated(Guid eventId, ApplicationMetadata metadata,
            Guid contractId, Guid clientId, decimal amount)
            : base(eventId, metadata)
        {
            ContractId = contractId;
            ClientId = clientId;
            Amount = amount;
        }

        public ContractValidated(Guid contractId, Guid clientId, decimal amount)
            : this(Guid.NewGuid(), new ApplicationMetadata { CreationDate = DateTime.UtcNow },
                contractId, clientId, amount)
        {
        }
    }
}
