using System;
using MediatR;

namespace NBB.Contracts.PublishedLanguage.IntegrationEvents
{
    public record ContractValidated(
        Guid ContractId,
        Guid ClientId,
        decimal Amount
    ) : INotification;
}
