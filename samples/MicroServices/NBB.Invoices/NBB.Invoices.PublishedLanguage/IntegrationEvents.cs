// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;

namespace NBB.Invoices.PublishedLanguage
{
    public record InvoiceCreated(Guid InvoiceId, decimal Amount, Guid ClientId, Guid? ContractId) : INotification;
    public record InvoiceMarkedAsPayed(Guid InvoiceId, Guid? ContractId) : INotification;
}