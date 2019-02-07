using MediatR;
using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace NBB.Application.DataContracts
{
    public abstract class Command : ICommand, IMetadataProvider<ApplicationMetadata>
    {
        public Guid CommandId { get; }
        public ApplicationMetadata Metadata { get; }

        protected Command(Guid commandId, ApplicationMetadata metadata)
        {
            CommandId = commandId;
            Metadata = metadata ?? new ApplicationMetadata();
        }
    }

    public abstract class Command<TResponse> : Command, IRequest<TResponse>
    {
        protected Command(Guid commandId, ApplicationMetadata metadata)
            : base(commandId, metadata)
        {
        }
    }
}
