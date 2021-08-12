// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using NBB.Contracts.Domain.ContractAggregate.Snapshots;
using NBB.Domain;

namespace NBB.Contracts.Domain.ContractAggregate
{
    public class Contract : EventSourcedAggregateRoot<Guid, ContractSnapshot>
    {
        public Guid ContractId { get; private set; }
        public decimal Amount { get; private set; }

        public Guid ClientId { get; private set; }

        public List<ContractLine> ContractLines { get; private set; } = new List<ContractLine>();

        public bool IsValidated { get; private set; }


        //needed 4 repository should be private
        public Contract()
        {
        }

        public Contract(Guid clientId)
        {
            Emit(new ContractCreated(Guid.NewGuid(), clientId));
        }

        public override Guid GetIdentityValue() => ContractId;


        public void AddContractLine(string product, decimal price, int quantity)
        {
            if (IsValidated)
                throw new Exception("Contract is validated");

            Emit(new ContractLineAdded(Guid.NewGuid(), this.ContractId, product, price, quantity));
            Emit(new ContractAmountUpdated(this.ContractId, this.Amount + price * quantity));
        }

        public void Validate()
        {
            if (IsValidated)
                throw new Exception("Contract already validated");
            Emit(new ContractValidated(this.ContractId, this.ClientId, this.Amount));
        }

        protected override void SetMemento(ContractSnapshot memento)
        {
            Amount = memento.Amount;
            ClientId = memento.ClientId;
            ContractId = memento.ContractId;
            IsValidated = memento.IsValidated;
            ContractLines = memento.ContractLines.Select(x =>
                    new ContractLine(
                        new Product(x.Product.Name, x.Product.Price),
                        x.Quantity, x.ContractId))
                .ToList();
        }

        protected override ContractSnapshot CreateMemento()
        {
            return new ContractSnapshot()
            {
                Amount = Amount,
                ClientId = ClientId,
                ContractId = ContractId,
                IsValidated = IsValidated,
                ContractLines = ContractLines.Select(x =>
                    new ContractSnapshot.ContractLine
                    {
                        ContractId = x.ContractId,
                        ContractLineId = x.ContractLineId,
                        Quantity = x.Quantity,
                        Product = new ContractSnapshot.Product
                        {
                            Name = x.Product.Name,
                            Price = x.Product.Price
                        }
                    }).ToList()
            };
        }

        private void Apply(ContractCreated e)
        {
            this.ContractId = e.ContractId;
            this.ClientId = e.ClientId;
        }

        private void Apply(ContractAmountUpdated e)
        {
            this.Amount = e.NewAmount;
        }

        private void Apply(ContractLineAdded e)
        {
            var contractLine = new ContractLine(new Product(e.Product, e.Price), e.Quantity, this.ContractId);
            this.ContractLines.Add(contractLine);
        }

        private void Apply(ContractValidated e)
        {
            this.IsValidated = true;
        }
    }
}