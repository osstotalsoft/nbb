namespace NBB.Contracts.FSharpMvc.Api.Controllers

open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Mvc
open MediatR
open NBB.Correlation
open NBB.Application.DataContracts


type MediatorControllerBase (_mediator: IMediator) =
    inherit ControllerBase()

    member this.MediatorSendQuery (query: IRequest<'Response>) =
        task { 
            let! result = _mediator.Send(query)
            return this.Ok(result)  :> IActionResult
        }

    member this.MediatorSendCommand (command: Command) =
        task {
            do! _mediator.Send(command)
            let result = {| CommandId = command.Metadata.CommandId; CorrelationId = CorrelationManager.GetCorrelationId() |}
            return this.Ok(result) :> IActionResult
        }