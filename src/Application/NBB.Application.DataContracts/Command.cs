using MediatR;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public abstract class Command : ICommand, IRequest, IMetadataProvider<CommandMetadata>
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    public abstract class Command<TResponse> : ICommand, IRequest<TResponse>, IMetadataProvider<CommandMetadata>
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }
}
