using System;

namespace ProcessManagerSample
{
    public record OrderProcessManagerData
    {
        public Guid OrderId { get; init; }
        public bool IsPaid { get; init; }
    }
}