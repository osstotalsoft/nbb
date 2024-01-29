// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module EventPipelineTests

open Xunit
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp
open FsUnit.Xunit
open NBB.Core.Effects.FSharp.Interpreter
open Mox

type SomeEvent =
    { Code: string; EventId: System.Guid }
    interface IEvent


type SomeOtherEvent =
    { Name: string; EventId: System.Guid }
    interface IEvent

open EventHandler
open FsCheck.Xunit


[<Fact>]
let ``EventHandler.choose should call the handlers until one returns some`` () =
    task {
        //arrange
        let handle1 = mock (fun (_: SomeEvent) -> effect { return None })
        let handle2 = mock (fun (_: SomeEvent) -> effect { return Some() })
        let handle3 = mock (fun (_: SomeEvent) -> effect { return Some() })

        let eventHandler=
            choose [
                handle1.Fn
                handle2.Fn
                handle3.Fn
            ]
        let interpreter = createInterpreter ()

        //act
        do!
            { SomeEvent.Code = ""
              EventId = System.Guid.NewGuid() }
            |> eventHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handle1 |> wasCalled |> should be True
        handle2 |> wasCalled |> should be True
        handle3 |> wasCalled |> should be False
    }

[<Fact>]
let ``EventHandler.upCast should call the handler if types match`` () =
    task {
        //arrange
        let handler = mock (fun (_: SomeEvent) -> effect { return Some() })
        let eventHandler: EventHandler = handler.Fn |> upCast
        let interpreter = createInterpreter ()

        //act
        do!
            { Code = ""
              EventId = System.Guid.NewGuid() }
            |> eventHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler |> wasCalled |> should be True
    }

[<Fact>]
let ``EventHandler.upCast should not call the handler if types don't match`` () =
    task {
        //arrange
        let handler = mock (fun (_: SomeEvent) -> effect { return Some() })
        let eventHandler: EventHandler = handler.Fn |> upCast
        let interpreter = createInterpreter ()

        //act
        do!
            { Name = ""
              EventId = System.Guid.NewGuid() }
            |> eventHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler |> wasCalled |> should be False
    }

[<Fact>]
let ``EventHandler.append should call both handlers`` () =
    task {
        //arrange
        let handler1 = mock (fun (_: SomeEvent) -> effect { return Some() })
        let handler2 = mock (fun (_: SomeEvent) -> effect { return Some() })
        let appendedHandler = handler1.Fn ++ handler2.Fn
        let interpreter = createInterpreter ()

        //act
        do!
            { Code = ""
              EventId = System.Guid.NewGuid() }
            |> appendedHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler1 |> wasCalled |> should be True
        handler2 |> wasCalled |> should be True
    }

[<Property>]
let ``EventHandler.append left identity law `` (f: SomeEvent -> unit option) (req: SomeEvent) =
    let f' = f >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> _.GetAwaiter().GetResult()


    run (empty ++ f') = run f'


[<Property>]
let ``EventHandler.append right identity law`` (f: SomeEvent -> unit option) (req: SomeEvent) =
    let f' = f >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> _.GetAwaiter().GetResult()

    run (f' ++ empty) = run f'


[<Property>]
let ``EventHandler.append associativity law``
    (f: SomeEvent -> unit option)
    (g: SomeEvent -> unit option)
    (h: SomeEvent -> unit option)
    (req: SomeEvent)
    =

    let f' = f >> Effect.pure'
    let g' = g >> Effect.pure'
    let h' = h >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> _.GetAwaiter().GetResult()

    run ((f' ++ g') ++ h') = run (f' ++ (g' ++ h'))


open EventMiddleware

[<Fact>]
let ``EventMiddleware.lift should always call the next handler`` () =
    task {
        //arrange
        let handler1 = mock (fun (_: SomeEvent) -> effect { return Some() })
        let handler2 = mock (fun (_: SomeEvent) -> effect { return None })
        let next = mock (fun (_: SomeEvent) -> effect { return Some() })
        let eventHandler = lift handler1.Fn <| lift handler2.Fn next.Fn
        let interpreter = createInterpreter ()

        //act
        do!
            { Code = ""; EventId = System.Guid.NewGuid() }
            |> eventHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler1 |> wasCalled |> should be True
        handler2 |> wasCalled |> should be True
        next |> wasCalled |> should be True
    }

[<Fact>]
let ``EventMiddleware.handlers should call each handler until one returns Some`` () =
    task {
        //arrange
        let handle1 = mock (fun (_: SomeEvent) -> effect { return None })
        let handle2 = mock (fun (_: SomeEvent) -> effect { return Some() })
        let handle3 = mock (fun (_: SomeEvent) -> effect { return Some() })

        let eventPipeline =
            handlers [ handle1.Fn |> upCast; handle2.Fn |> upCast; handle3.Fn |> upCast ]

        let interpreter = createInterpreter ()

        //act
        do!
            { Code = ""; EventId = System.Guid.NewGuid() }
            |> run eventPipeline
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handle1 |> wasCalled |> should be True
        handle2 |> wasCalled |> should be True
        handle3 |> wasCalled |> should be False
    }
