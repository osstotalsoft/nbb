// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Api.Handlers

open Giraffe
open NBB.Invoices.FSharp.Application
open NBB.Application.Mediator.FSharp

module Invoice =
    let handler : HttpHandler =
        subRoute "/invoices" (
            choose [
                POST >=> route  "/create"  >=> bindJson<CreateInvoice.Command> (Mediator.sendCommand |> HandlerUtils.interpretCommand)
                POST >=> route  "/pay"  >=> bindJson<MarkInvoiceAsPayed.Command> (Mediator.sendCommand |> HandlerUtils.interpretCommand)
            ])

