// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Contracts.ReadModel
{
    public class ContractLineReadModel
    {
        public Guid ContractLineId { get; set; }
        public string Product { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public Guid ContractId { get; set; }


        private ContractLineReadModel()
        {

        }

        public ContractLineReadModel(Guid contractLineId, string product, decimal price, int quantity, Guid contractId)
        {
            ContractLineId = contractLineId;
            Product = product;
            Price = price;
            Quantity = quantity;
            ContractId = contractId;
        }
    }
}
