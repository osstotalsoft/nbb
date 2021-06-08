namespace NBB.Invoices.FSharp.Api.Handlers

open Giraffe
open NBB.Invoices.FSharp.Application

module Invoice =
    let handler : HttpHandler = 
        subRoute "/invoices" (
            choose [
                POST >=> route  "/"  >=> bindJson<CreateInvoice.Command> (Mediator.sendCommand |> HandlerUtils.interpretCommand)
            ])

