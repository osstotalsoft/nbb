[<AutoOpen>]
module NBB.Contracts.Handlers.ContractsAlternative

open Giraffe
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries
open NBB.HandlerUtils


let getByIdCustomQuery id =
    let query = GetContractById.Query(Id = id)
    handleQuery query

let getByIdOverrideFromRoute id  =
    bindModel<GetContractById.Query> None (fun query ->
        query.Id <- id
        handleQuery query
    )

let contractsAlternativeHandler : HttpHandler =  
    subRoute "/contracts2" (
        choose [
            GET >=> choose [
                route "" >=> bindModel<GetContracts.Query> None handleQuery
                routef "/%O" getByIdOverrideFromRoute
            ]
            POST >=> choose [
                route "" >=> bindModel<CreateContract> None handleCommand
                routex "/(.*)/lines" >=> bindModel<AddContractLine> None handleCommand
                routex "/(.*)/validate" >=> bindModel<ValidateContract> None handleCommand
            ]
        ])

