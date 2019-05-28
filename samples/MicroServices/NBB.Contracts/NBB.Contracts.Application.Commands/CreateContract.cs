using NBB.Application.DataContracts;
using System;

namespace NBB.Contracts.Application.Commands
{
    public class CreateContract : Command
    {
        public Guid ClientId { get; }

        public CreateContract(Guid clientId, CommandMetadata metadata = null)
            : base(metadata)
        {
            ClientId = clientId;
        }

    }
}