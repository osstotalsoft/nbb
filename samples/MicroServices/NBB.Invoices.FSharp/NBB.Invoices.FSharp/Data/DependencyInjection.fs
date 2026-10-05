// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Data

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open NBB.Core.Effects
open NBB.Invoices.FSharp.Domain
open NBB.Invoices.FSharp.Application

module DataAccess =
    let private handler (handle: 'se -> CancellationToken -> Task<'a>) =
        Func<'se, CancellationToken, Task<'a>>(handle)

    /// Registers the SQL Server side effect handlers; reads ConnectionStrings:DefaultConnection from the configuration
    let addServices (configuration: IConfiguration) (services: IServiceCollection) =
        let connectionString () = configuration.GetConnectionString "DefaultConnection"

        services
            .AddSideEffectHandler(handler (InvoiceRepoImpl.handle<InvoiceAggregate.Invoice option> connectionString))
            .AddSideEffectHandler(handler (InvoiceRepoImpl.handle<unit> connectionString))
            .AddSideEffectHandler(handler (InvoiceReadModelImpl.handle<InvoiceView list> connectionString))
            .AddSideEffectHandler(handler (InvoiceReadModelImpl.handle<InvoiceView option> connectionString))
