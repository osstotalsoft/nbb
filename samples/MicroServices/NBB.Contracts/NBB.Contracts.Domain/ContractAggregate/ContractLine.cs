using System;
using NBB.Domain;

namespace NBB.Contracts.Domain.ContractAggregate
{
    public class ContractLine : Entity<Guid>
    {
        public Guid ContractLineId { get; private set; }
        public Product Product { get; private set; }
        public int Quantity { get; private set; }

        public Guid ContractId { get; private set; }


        internal ContractLine(Product product, int quantity, Guid contractId)
        {
            Product = product;
            Quantity = quantity;
            ContractId = contractId;
        }

        public override Guid GetIdentityValue() => ContractLineId;
    }
}
