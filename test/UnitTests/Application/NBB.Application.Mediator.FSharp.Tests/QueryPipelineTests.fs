// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module QueryPipelineTests

open Xunit
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp
open FsUnit.Xunit
open NBB.Core.Effects.FSharp.Interpreter
open Mox
open QueryHandler

type SomeQuery =
    { Code: string }
    interface IQuery<bool>


type SomeOtherQuery =
    { Name: string }
    interface IQuery<bool>


[<Fact>]
let ``QueryHandler.upCast should call the handler if types match`` () =
    task {
        //arrange
        let handler = mock (fun (_: SomeQuery) -> effect { return Some true })
        let queryHandler: QueryHandler = handler.Fn |> upCast
        let interpreter = createInterpreter ()

        //act
        do!
            { Code = "" }
            |> queryHandler |> Effect.interpret interpreter |> Task.ignore

        //assert
        handler |> wasCalled |> should be True
    }

[<Fact>]
let ``QueryHandler.upCast should not call the handler if types don't match`` () =
    task {
        //arrange
        let handler = mock (fun (_: SomeQuery) -> effect { return Some true })
        let queryHandler: QueryHandler = handler.Fn |> upCast
        let interpreter = createInterpreter ()

        //act
        do!
            { Name = "" }
            |> queryHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler |> wasCalled |> should be False
    }
