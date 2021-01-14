﻿using System;
using MediatR;

namespace NBB.Invoices.Application.Commands
{
    public record CreateInvoice (
        decimal Amount,
        Guid ClientId,
        Guid? ContractId
    ) : IRequest;

    //string IKeyProvider.Key => ContractId.ToString();
}
