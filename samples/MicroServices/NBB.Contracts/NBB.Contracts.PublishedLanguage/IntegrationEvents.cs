using System;
using MediatR;

namespace NBB.Contracts.PublishedLanguage
{
    public record ContractValidated(Guid ContractId, Guid ClientId, decimal Amount) : INotification;
}