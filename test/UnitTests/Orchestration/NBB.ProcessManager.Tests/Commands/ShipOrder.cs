using System;
using NBB.Core.Abstractions;

namespace NBB.ProcessManager.Tests.Commands
{
    public class ShipOrder : ICommand
    {
        public Guid OrderId { get; }
        public decimal Amount { get; }
        public string ShippingAddress{ get; }

        public ShipOrder(Guid orderId, decimal amount, string shippingAddress)
        {
            OrderId = orderId;
            Amount = amount;
            ShippingAddress = shippingAddress;
        }
    }
}
