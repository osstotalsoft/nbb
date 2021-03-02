using MediatR;
using System;

namespace NBB.Application.DataContracts
{
    public abstract class Command : IRequest
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    public abstract class Command<TResponse> : IRequest<TResponse>
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    public class CommandMetadata
    {
        public Guid CommandId { get; }
        public DateTime CreationDate { get; }

        public CommandMetadata(Guid commandId, DateTime creationDate)
        {
            CommandId = commandId;
            CreationDate = creationDate;
        }

        public static CommandMetadata Default() => new CommandMetadata(Guid.NewGuid(), DateTime.UtcNow);
    }
}
