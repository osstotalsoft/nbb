namespace NBB.Application.Mediator.FSharp

open Microsoft.Extensions.DependencyInjection
open NBB.Core.Effects
open PipelineUtils

[<AutoOpen>]
module ServiceCollectionExtensions =
    type IServiceCollection with
        member this.AddMediator(commandPipeline, queryPipeline, eventPipeline) = 

            let sendCommand (cmd: 'TCommand) =
                CommandMiddleware.run commandPipeline cmd
                |> terminateRequest

            let publishEvent (ev: 'TEvent) =
                EventMiddleware.run eventPipeline ev
                |> terminateEvent

            let sendQuery (q: IQuery) =
                RequestMiddleware.run queryPipeline q
                |> terminateRequest<obj>

            let mediator =
                { SendCommand = sendCommand
                  SendQuery = sendQuery
                  DispatchEvent = publishEvent }

            this.AddSideEffectHandler(Mediator.handleGetMediator mediator)            