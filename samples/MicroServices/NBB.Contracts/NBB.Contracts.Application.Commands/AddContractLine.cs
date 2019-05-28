using NBB.Application.DataContracts;
using System;

namespace NBB.Contracts.Application.Commands
{
    public class AddContractLine : Command
    {
        public string Product { get; }

        public decimal Price { get; }

        public int Quantity { get; }

        public Guid ContractId { get; }


        public AddContractLine(string product, decimal price, int quantity, Guid contractId, CommandMetadata metadata = null)
            : base(metadata)
        {
            Product = product;
            Price = price;
            Quantity = quantity;
            ContractId = contractId;
        }
    }
}