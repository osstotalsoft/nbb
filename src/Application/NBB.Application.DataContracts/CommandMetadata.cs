using System;

namespace NBB.Application.DataContracts
{
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
