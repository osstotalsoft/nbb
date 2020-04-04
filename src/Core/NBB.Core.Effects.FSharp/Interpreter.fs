namespace NBB.Core.Effects.FSharp

open NBB.Core.Effects
open System.Threading.Tasks

module Interpreter =
    type HandlerFunc<'TSideEffect, 'TOutput when 'TSideEffect:> ISideEffect<'TOutput>> = ('TSideEffect -> 'TOutput)
    
    type HandlerWrapper<'TSideEffect, 'TOutput when 'TSideEffect:> ISideEffect<'TOutput>> (handlerFunc : HandlerFunc<'TSideEffect, 'TOutput>) = 
        interface ISideEffectHandler<ISideEffect<'TOutput>,'TOutput> with
            member _.Handle(sideEffect, _cancellationToken) = 
                match sideEffect with
                    | :? 'TSideEffect as sideEffect -> handlerFunc(sideEffect) |> Task.FromResult
                    | _ -> failwith "Wrong type"


    type SideEffectHandlerFactory() =
        interface ISideEffectHandlerFactory with
            member _.GetSideEffectHandlerFor<'TOutput>(sideEffect) = 
                let thunkHandler = Thunk.Handler()
                let handleThunk = thunkHandler.Handle >> Async.AwaitTask >> Async.RunSynchronously// >> (fun _unit -> Unit())
                match sideEffect with
                | :? Thunk.SideEffect<'TOutput> -> HandlerWrapper(handleThunk):> ISideEffectHandler<ISideEffect<'TOutput>,'TOutput>
                | _ -> failwith "Invalid sideEffect"
             

    let createInterpreter = SideEffectHandlerFactory >> Interpreter
