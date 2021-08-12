// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module CommandPipelineTests

open Xunit
open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open NBB.Application.Mediator.FSharp
open FsUnit.Xunit
open NBB.Core.Effects.FSharp.Interpreter
open Mox
open CommandHandler

type SomeCommand = { Code: string } interface ICommand
type SomeOtherCommand = { Name: string } interface ICommand

[<Fact>]
let ``CommandHandler.upCast should call the handler if types match`` () =
    //arrange
    let handler = mock (fun (_:SomeCommand) -> effect { return Some () })
    let commandHandler: CommandHandler = handler.Fn |> upCast
    let interpreter = createInterpreter()

    //act
    {Code = ""}
        |> commandHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler |> wasCalled |> should be True

[<Fact>]
let ``CommandHandler.upCast should not call the handler if types don't match`` () =
    //arrange
    let handler = mock (fun (_:SomeCommand) -> effect { return Some () })
    let commandHandler: CommandHandler = handler.Fn |> upCast
    let interpreter = createInterpreter()

    //act
    {Name = ""}
        |> commandHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler |> wasCalled |> should be False

