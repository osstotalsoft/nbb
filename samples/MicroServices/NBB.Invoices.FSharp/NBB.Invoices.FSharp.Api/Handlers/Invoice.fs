namespace NBB.Invoices.FSharp.Api.Handlers

open Giraffe
open NBB.Invoices.FSharp.Application

module Invoice =
    let handler : HttpHandler = 
        subRoute "/invoices" (
            choose [
                POST >=> route  "/create"  >=> bindJson<CreateInvoice.Command> (Mediator.sendCommand |> HandlerUtils.interpretCommand)
                POST >=> route  "/pay"  >=> bindJson<MarkInvoiceAsPayed.Command> (Mediator.sendCommand |> HandlerUtils.interpretCommand)
            ])

