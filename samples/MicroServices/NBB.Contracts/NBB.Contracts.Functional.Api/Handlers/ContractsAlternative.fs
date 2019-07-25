namespace NBB.Contracts.Handlers

open Giraffe
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries
open NBB.HandlerUtils

module ContractsAlternative = 

    let private getByIdCustomQuery id =
        let query = GetContractById.Query(Id = id)
        mediatorSendQuery query

    let private getByIdOverrideFromRoute id  =
        bindModel<GetContractById.Query> None (fun query ->
            query.Id <- id
            mediatorSendQuery query
    )

    let handler : HttpHandler =  
        subRoute "/contracts2" (
            choose [
                GET >=> choose [
                    route "" >=> bindModel<GetContracts.Query> None mediatorSendQuery
                    routef "/%O" getByIdOverrideFromRoute
                ]
                POST >=> choose [
                    route "" >=> bindJson<CreateContract> mediatorSendCommand
                    routex "/(.*)/lines" >=> bindJson<AddContractLine> mediatorSendCommand
                    routex "/(.*)/validate" >=> bindJson<ValidateContract> mediatorSendCommand
                ]
            ])

