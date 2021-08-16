// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;

namespace NBB.Contracts.PublishedLanguage
{
    public record ContractValidated(Guid ContractId, Guid ClientId, decimal Amount) : INotification;
}