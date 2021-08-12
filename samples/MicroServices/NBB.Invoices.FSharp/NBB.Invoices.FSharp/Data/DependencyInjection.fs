// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Data
open NBB.Core.Effects
open Microsoft.Extensions.DependencyInjection
open NBB.Invoices.FSharp.Domain

module DataAccess =
    let addServices (services:IServiceCollection) =
        services
            .AddSideEffectHandler(InvoiceRepoImpl.handle<InvoiceAggregate.Invoice>)
            .AddSideEffectHandler(InvoiceRepoImpl.handle<unit>)



