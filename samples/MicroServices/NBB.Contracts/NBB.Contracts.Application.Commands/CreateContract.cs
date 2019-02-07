using NBB.Application.DataContracts;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Application.Commands
{
    public class CreateContract : Command
    {
        public Guid ClientId { get; }

        [JsonConstructor]
        private CreateContract(Guid clientId, Guid commandId, ApplicationMetadata metadata)
            : base(commandId, metadata)
        {
            ClientId = clientId;
        }

        public CreateContract(Guid clientId)
            : this(clientId, Guid.NewGuid(), new ApplicationMetadata { CreationDate = DateTime.UtcNow })
        {
        }

    }
}