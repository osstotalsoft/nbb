using System;
using System.Collections.Generic;

namespace NBB.Contracts.Domain.ContractAggregate.Snapshots
{
    public class ContractSnapshot
    {
        public Guid ContractId { get; set; }
        public decimal Amount { get; set; }
        public Guid ClientId { get;  set; }
        public List<ContractLine> ContractLines { get; set; }
        public bool IsValidated { get; set; }

        public class ContractLine 
        {
            public Guid ContractLineId { get; set; }
            public Product Product { get; set; }
            public int Quantity { get; set; }
            public Guid ContractId { get; set; }
        }

        public class Product
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }
    }
}
