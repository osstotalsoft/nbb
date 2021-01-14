﻿using System;
using MediatR;

namespace NBB.Invoices.PublishedLanguage
{
    public record InvoiceCreated(Guid InvoiceId, decimal Amount, Guid ClientId, Guid? ContractId) : INotification;
}