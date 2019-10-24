using System;

namespace ProcessManagerSample
{
    public struct OrderProcessManagerData
    {
        public Guid OrderId { get; set; }
        public bool IsPaid { get; set; }
    }
}