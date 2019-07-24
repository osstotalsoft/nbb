[<AutoOpen>]
module NBB.Contracts.Handlers.Contracts

open Giraffe
open NBB.HandlerUtils
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries


let contractsHandler : HttpHandler =  
    subRoute "/contracts" (
        choose [
            GET >=> route "" >=> bindQuery<GetContracts.Query> mediatorSendQuery
            GET >=> route "/(.*)" >=> bindQuery<GetContractById.Query> mediatorSendQuery 
            POST >=> route "" >=> bindJson<CreateContract> mediatorSendCommand
            POST >=> routex "/(.*)/lines" >=> bindJson<CreateContract> mediatorSendCommand
            POST >=> routex "/(.*)/validate" >=> bindJson<CreateContract> mediatorSendCommand
        ])


