// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;

namespace NBB.Payments.PublishedLanguage
{
    public record CreatePayable(Guid ClientId, decimal Amount, Guid InvoiceId, Guid? ContractId) : IRequest;
    public record PayPayable(Guid PayableId) : IRequest;
}