namespace NBB.Contracts.Handlers

open Giraffe
open NBB.HandlerUtils
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries


module Contracts =
    let handler : HttpHandler =  
       subRoute "/contracts" (
            choose [
                GET     >=> route  ""                >=>  bindQuery<GetContracts.Query>    mediatorSendQuery
                GET     >=> routex "/(.*)"           >=>  bindQuery<GetContractById.Query> mediatorSendQuery 
                POST    >=> route  ""                >=>  bindJson<CreateContract>         mediatorSendCommand
                POST    >=> routex "/(.*)/lines"     >=>  bindJson<CreateContract>         mediatorSendCommand
                POST    >=> routex "/(.*)/validate"  >=>  bindJson<CreateContract>         mediatorSendCommand
            ])


