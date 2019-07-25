namespace NBB.Contracts.FSharpMvc.Api.Controllers

open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Mvc
open MediatR
open NBB.Contracts.Application.Queries
open NBB.Contracts.Application.Commands
open System.Threading
open NBB.Correlation
open System.Threading.Tasks

[<Route("api/[controller]")>]
[<ApiController>]
type ContractsController (_mediator: IMediator) =
    inherit ControllerBase()

    [<HttpGet>]
    member this.Get([<FromRoute>] query: GetContracts.Query) =
        task {
            let! result = _mediator.Send(query);
            return this.Ok result
        }
  
    [<HttpGet("{id}")>]
    member this.Get([<FromRoute>] query: GetContractById.Query) : Task<IActionResult> =
        task {
            let! contract = _mediator.Send(query)

            if (not (isNull contract)) then
                return this.Ok(contract) :> IActionResult
            else
                return this.NotFound() :> IActionResult
        }

    [<HttpPost>]
    member this.Post([<FromBody>] command:CreateContract, cancellationToken: CancellationToken) =
        task {
            do! _mediator.Send(command, cancellationToken);
            let result = {| CommandId = command.Metadata.CommandId; CorrelationId = CorrelationManager.GetCorrelationId() |}
            return this.Ok(result)
        }

    [<HttpPost("{id}/lines")>]
    member this.Post([<FromBody>] command:AddContractLine, cancellationToken: CancellationToken) =
        task {
            do! _mediator.Send(command, cancellationToken);
            let result = {| CommandId = command.Metadata.CommandId; CorrelationId = CorrelationManager.GetCorrelationId() |}
            return this.Ok(result)
        }

    [<HttpPost("{id}/validate")>]
    member this.Post([<FromBody>] command:ValidateContract, cancellationToken: CancellationToken) =
        task {
            do! _mediator.Send(command, cancellationToken);
            let result = {| CommandId = command.Metadata.CommandId; CorrelationId = CorrelationManager.GetCorrelationId() |}
            return this.Ok(result)
        }
