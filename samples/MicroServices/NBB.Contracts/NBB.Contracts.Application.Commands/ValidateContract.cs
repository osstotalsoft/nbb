using NBB.Application.DataContracts;
using NBB.Core.Abstractions;
using Newtonsoft.Json;
using System;

namespace NBB.Contracts.Application.Commands
{
    public class ValidateContract : Command, IKeyProvider
    {
        public Guid ContractId { get; }

        string IKeyProvider.Key => ContractId.ToString();

        public ValidateContract(Guid contractId, CommandMetadata metadata = null)
            : base(metadata)
        {
            ContractId = contractId;
        }
    }
}
