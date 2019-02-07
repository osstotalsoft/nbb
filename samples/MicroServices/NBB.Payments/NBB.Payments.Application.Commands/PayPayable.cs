using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using Newtonsoft.Json;
using System;

namespace NBB.Payments.Application.Commands
{
    public class PayPayable : Command, IKeyProvider
    {
        public Guid PayableId { get; }

        string IKeyProvider.Key => PayableId.ToString();

        [JsonConstructor]
        private PayPayable(Guid commandId, ApplicationMetadata metadata, Guid payableId)
            : base(commandId, metadata)
        {
            PayableId = payableId;
       }

        public PayPayable(Guid payableId, Guid? correlationId)
            : this(Guid.NewGuid(),
                new ApplicationMetadata { CreationDate = DateTime.UtcNow },
                payableId)
        {
            PayableId = payableId;
        }
    }
}
