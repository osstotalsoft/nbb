using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using System;

namespace NBB.Payments.Application.Commands
{
    public class PayPayable : Command, IKeyProvider
    {
        public Guid PayableId { get; }

        string IKeyProvider.Key => PayableId.ToString();

        public PayPayable(Guid payableId, CommandMetadata metadata = null)
            : base(metadata)
        {
            PayableId = payableId;
        }
    }
}
