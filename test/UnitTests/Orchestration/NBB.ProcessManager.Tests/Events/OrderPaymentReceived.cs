using System;
using NBB.Application.DataContracts;

namespace NBB.ProcessManager.Tests.Events
{
    public class OrderPaymentReceived : Event
    {
        public Guid ContractOrderId { get; }

        public OrderPaymentReceived(Guid contractOrderId)
        {
            ContractOrderId = contractOrderId;
        }

    }
}