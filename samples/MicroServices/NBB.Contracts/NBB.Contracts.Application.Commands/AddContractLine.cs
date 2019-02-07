using NBB.Application.DataContracts;
using NBB.Messaging.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NBB.Contracts.Application.Commands
{
    public class AddContractLine : Command
    {
        public string Product { get; }

        public decimal Price { get; }

        public int Quantity { get; }

        public Guid ContractId { get; }


        [JsonConstructor]
        private AddContractLine(string product, decimal price, int quantity, Guid contractId, Guid commandId, ApplicationMetadata metadata)
            : base(commandId, metadata)
        {
            Product = product;
            Price = price;
            Quantity = quantity;
            ContractId = contractId;
        }
    }
}