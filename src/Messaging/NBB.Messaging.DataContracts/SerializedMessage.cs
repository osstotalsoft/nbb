using NBB.Core.Abstractions;
using System;

namespace NBB.Messaging.DataContracts
{
    public abstract class SerializedMessage : ICorrelatable, IKeyProvider
    {
        public string Body { get; }
        public string TypeId { get; }
        public Guid? CorrelationId { get; set; }
        public string Key { get; }
        public Guid MessageId { get; }
        public DateTime CreationDate { get; }

        protected SerializedMessage(string body, string typeId, Guid messageId, DateTime creationDate, Guid? correlationId, string key)
        {
            Body = body;
            TypeId = typeId;
            MessageId = messageId;
            CreationDate = creationDate;
            CorrelationId = correlationId;
            Key = key;
        }
    }
}