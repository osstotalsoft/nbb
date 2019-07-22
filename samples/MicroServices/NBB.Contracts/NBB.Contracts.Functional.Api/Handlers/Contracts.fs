[<AutoOpen>]
module NBB.Contracts.Handlers.Contracts

open System.Collections.Generic
open Giraffe
open NBB.HandlerUtils
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries
open NBB.Contracts.ReadModel


let contractsHandler : HttpHandler =  
    subRoute "/contracts" (
        choose [
            GET >=> choose [
                route "" >=> handleQueryFromRequest<GetContracts.Query, List<ContractReadModel>>
                routeBindNonStrict<GetContractById.Query> "/{id}" handleQuery 
            ]
            POST >=> choose [
                route "" >=> handleCommandFromRequest<CreateContract>
                routex "/(.*)/lines" >=> handleCommandFromRequest<AddContractLine>
                routex "/(.*)/validate" >=> handleCommandFromRequest<ValidateContract>
            ]
        ])


