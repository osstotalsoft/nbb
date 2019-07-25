namespace NBB.Contracts.FSharpMvc.Api.Controllers

open Microsoft.AspNetCore.Mvc
open MediatR
open NBB.Contracts.Application.Queries
open NBB.Contracts.Application.Commands

[<Route("api/[controller]")>]
[<ApiController>]
type ContractsAlternativeController (_mediator: IMediator) =
    inherit MediatorControllerBase(_mediator)

    [<HttpGet>]
    member this.Get([<FromRoute>] query: GetContracts.Query) = this.MediatorSendQuery query

    [<HttpGet("{id}")>]
    member this.Get([<FromRoute>] query: GetContractById.Query) = this.MediatorSendQuery query

    [<HttpPost>]
    member this.Post([<FromBody>] command: CreateContract) = this.MediatorSendCommand command

    [<HttpPost("{id}/lines")>]
    member this.Post([<FromBody>] command: AddContractLine) = this.MediatorSendCommand command

    [<HttpPost("{id}/validate")>]
    member this.Post([<FromBody>] command: ValidateContract) = this.MediatorSendCommand command


