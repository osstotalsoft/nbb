namespace NBB.Core.Effects.FSharp

open Microsoft.Extensions.DependencyInjection


module Interpreter =
    let createInterpreter () = NBB.Core.Effects.Interpreter.CreateDefault(null)
    let createInterpreterWith (configureServices:IServiceCollection -> unit) = NBB.Core.Effects.Interpreter.CreateDefault(System.Action<IServiceCollection>(configureServices))
