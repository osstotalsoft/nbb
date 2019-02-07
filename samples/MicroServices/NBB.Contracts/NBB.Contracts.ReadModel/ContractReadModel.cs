using System;
using System.Collections.Generic;

namespace NBB.Contracts.ReadModel
{
    public class ContractReadModel
    {
        public Guid ContractId { get; set; }

        public decimal Amount { get; set; }

        public Guid ClientId { get; set; }

        public int Version { get; set; }

        public List<ContractLineReadModel> ContractLines { get; } = new List<ContractLineReadModel>();

        public bool IsValidated { get; set; }

        private ContractReadModel()
        {
        }
        public ContractReadModel(Guid contractId, Guid clientId, int version)
        {
            ContractId = contractId;
            ClientId = clientId;
            Version = version;


        }
    }
}
