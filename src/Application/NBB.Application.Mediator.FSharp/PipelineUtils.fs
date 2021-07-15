
namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp

module PipelineUtils =
    let terminateRequest<'a> : (Effect<'a option> -> Effect<'a>) =
        Effect.map
            (function
            | Some value -> value
            | None -> failwith "No handler found")

    let terminateEvent : (Effect<unit option> -> Effect<unit>) = Effect.map ignore

