using MediatR;
using NBB.Core.Abstractions;
using System;

namespace NBB.Application.DataContracts
{
    public abstract class Command : ICommand, IMetadataProvider<CommandMetadata>
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    public abstract class Command<TResponse> : Command, IRequest<TResponse>
    {
        protected Command(CommandMetadata metadata)
            : base(metadata)
        {
        }
    }
}
