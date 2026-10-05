// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Api.Handlers

open System
open Giraffe
open NBB.Application.Mediator.FSharp
open NBB.Invoices.FSharp.Application
open NBB.Invoices.PublishedLanguage
open HandlerUtils

/// The same routes as the C# Invoices API (NBB.Invoices.Api InvoicesController)
module Invoice =
    let handler: HttpHandler =
        subRouteCi
            "/invoices"
            (choose [ GET >=> routeCi "" >=> (Mediator.sendQuery GetInvoices.Query |> interpret json)
                      GET
                      >=> routeCif "/%O" (fun (invoiceId: Guid) ->
                          Mediator.sendQuery { GetInvoice.Query.InvoiceId = invoiceId } |> interpret jsonOrNotFound)
                      POST >=> routeCi "" >=> bindJson<CreateInvoice> publish
                      POST >=> routeCif "/%O/process" (fun (_: Guid) -> bindJson<ProcessInvoice> publish) ])
