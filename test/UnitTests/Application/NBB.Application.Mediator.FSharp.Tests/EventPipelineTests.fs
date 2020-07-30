module EventPipelineTests

open Xunit
open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open NBB.Application.Mediator.FSharp
open FsUnit.Xunit
open NBB.Core.Effects.FSharp.Interpreter
open Mox

type Event1 =
    { Code: string; EventId: System.Guid }
    interface IEvent with
        member this.EventId = this.EventId

type Event2 =
    { Name: string; EventId: System.Guid }
    interface IEvent with
        member this.EventId = this.EventId



open EventHandler



[<Fact>]
let ``EventHandler.choose should call the handlers until one returns some`` () =
    //arrange
    let handle1 = mock (fun (_:Event1) -> effect { return None})
    let handle2 = mock (fun (_:Event1) -> effect { return Some ()})
    let handle3 = mock (fun (_:Event1) -> effect { return Some ()})

    let eventHandler= 
        choose [
            handle1.Fn
            handle2.Fn
            handle3.Fn
        ]
    let interpreter = createInterpreter()

    //act
    {Event1.Code = ""; EventId = System.Guid.NewGuid()}
        |> eventHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handle1 |> wasCalled |> should be True
    handle2 |> wasCalled |> should be True
    handle3 |> wasCalled |> should be False

[<Fact>]
let ``EventHandler.upCast should call the handler if types match`` () =
    //arrange
    let handler = mock (fun (_:Event1) -> effect { return Some () })
    let eventHandler: EventHandler = handler.Fn |> upCast
    let interpreter = createInterpreter()

    //act
    {Code = ""; EventId = System.Guid.NewGuid()}
        |> eventHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler |> wasCalled |> should be True

[<Fact>]
let ``EventHandler.upCast should not call the handler if types don't match`` () =
    //arrange
    let handler = mock (fun (_:Event1) -> effect { return Some () })
    let eventHandler: EventHandler = handler.Fn |> upCast
    let interpreter = createInterpreter()

    //act
    {Name = ""; EventId = System.Guid.NewGuid()}
        |> eventHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler |> wasCalled |> should be False

[<Fact>]
let ``EventHandler.mappend should call both handlers`` () =
    //arrange
    let handler1 = mock (fun (_:Event1) -> effect { return Some () })
    let handler2 = mock (fun (_:Event1) -> effect { return Some () })
    let appendedHandler = handler1.Fn ++ handler2.Fn
    let interpreter = createInterpreter()

    //act
    {Code = ""; EventId = System.Guid.NewGuid()}
        |> appendedHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler1 |> wasCalled |> should be True
    handler2 |> wasCalled |> should be True

open EventMiddleware

[<Fact>]
let ``EventMiddleware.lift should always call the next handler`` () =
    //arrange
    let handler1 = mock (fun (_:Event1) -> effect { return Some () })
    let handler2 = mock (fun (_:Event1) -> effect { return None })
    let next = mock (fun (_:Event1) -> effect { return Some () })
    let eventHandler = lift handler1.Fn <| lift handler2.Fn next.Fn
    let interpreter = createInterpreter()

    //act
    {Code = ""; EventId = System.Guid.NewGuid()}
        |> eventHandler
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handler1 |> wasCalled |> should be True
    handler2 |> wasCalled |> should be True
    next |> wasCalled |> should be True

[<Fact>]
let ``EventMiddleware.handlers should call each handler until one returns Some``() =
    //arrange
    let handle1 = mock (fun (_:Event1) -> effect { return None })
    let handle2 = mock (fun (_:Event1) -> effect { return Some () })
    let handle3 = mock (fun (_:Event1) -> effect { return Some () })

    let eventPipeline = handlers [
        handle1.Fn |> upCast
        handle2.Fn |> upCast
        handle3.Fn |> upCast
    ]

    let interpreter = createInterpreter()
    
    //act
    {Code=""; EventId = System.Guid.NewGuid()}
        |> run eventPipeline
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> ignore

    //assert
    handle1 |> wasCalled |> should be True
    handle2 |> wasCalled |> should be True
    handle3 |> wasCalled |> should be False
