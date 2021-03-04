using MediatR;
using System;

namespace NBB.Application.DataContracts
{
    [Obsolete("Ensures NBB4 compatibility. Use IRequest instead.")]
    public abstract class Command : IRequest
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    [Obsolete("Ensures NBB4 compatibility. Use IRequest<TResponse> instead")]
    public abstract class Command<TResponse> : IRequest<TResponse>
    {
        public CommandMetadata Metadata { get; }

        protected Command(CommandMetadata metadata = null)
        {
            Metadata = metadata ?? CommandMetadata.Default();
        }
    }

    [Obsolete("Ensures NBB4 compatibility")]
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
