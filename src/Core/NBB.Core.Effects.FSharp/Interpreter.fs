namespace NBB.Core.Effects.FSharp

open NBB.Core.Effects

module Interpreter =

    type SideEffectBroker() =
        interface ISideEffectBroker with
            member _.Run((sideEffect: ISideEffect<'a>), cancellationToken) =
                match sideEffect with
                | :? Thunk.SideEffect<'a> -> Thunk.Handler<'a>().Handle(sideEffect:?> Thunk.SideEffect<'a>, cancellationToken)
                | _ -> failwith "Invalid sideEffect"
             

    let createInterpreter = SideEffectBroker >> Interpreter
