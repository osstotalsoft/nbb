using System;
using MediatR;

namespace NBB.Contracts.Domain.ContractAggregate
{
    public record ContractAmountUpdated(Guid ContractId, decimal NewAmount) : INotification;

    public record ContractCreated(Guid ContractId, Guid ClientId) : INotification;

    public record ContractLineAdded(Guid ContractLineId, Guid ContractId, string Product, decimal Price, int Quantity) : INotification;

    public record ContractValidated(Guid ContractId, Guid ClientId, decimal Amount) : INotification;
}