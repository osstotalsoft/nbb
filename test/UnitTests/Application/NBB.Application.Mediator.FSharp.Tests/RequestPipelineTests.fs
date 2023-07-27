// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module RequestPipelineTests

open Xunit
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp
open FsUnit.Xunit
open NBB.Core.Effects.FSharp.Interpreter
open Mox
open FsCheck.Xunit

type SomeRequest = { Code: string }


open RequestHandler

[<Fact>]
let ``RequestHandler.choose should call the handlers until one returns some`` () =
    task {
        //arrange
        let handle1 = mock (fun (_: SomeRequest) -> effect { return None })
        let handle2 = mock (fun (_: SomeRequest) -> effect { return Some() })
        let handle3 = mock (fun (_: SomeRequest) -> effect { return Some() })

        let requestHandler = choose [ handle1.Fn; handle2.Fn; handle3.Fn ]
        let interpreter = createInterpreter ()

        //act
        do! { Code = "" } |> requestHandler |> Effect.interpret interpreter |> Task.ignore

        //assert
        handle1 |> wasCalled |> should be True
        handle2 |> wasCalled |> should be True
        handle3 |> wasCalled |> should be False
    }

[<Fact>]
let ``RequestHandler.compose should call the second handler only if the first one returns Some`` () =
    task {
        //arrange
        let handle1 = mock (fun (_: SomeRequest) -> effect { return Some true })
        let handle2 = mock (fun (_: bool) -> effect { return None })
        let handle3 = mock (fun (_: string) -> effect { return Some() })

        let requestHandler = handle1.Fn >=> handle2.Fn >=> handle3.Fn
        let interpreter = createInterpreter ()

        //act
        do! { Code = "" } |> requestHandler |> Effect.interpret interpreter |> Task.ignore

        //assert
        handle1 |> wasCalled |> should be True
        handle2 |> wasCalled |> should be True
        handle3 |> wasCalled |> should be False
    }

[<Property>]
let ``RequestHandler.compose left identity law `` (f: int -> int option) (req: int) =
    let f' = f >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> fun t -> t.GetAwaiter().GetResult()

    run (identity >=> f') = run f'

[<Property>]
let ``RequestHandler.compose right identity law`` (f: int -> int option) (req: int) =
    let f' = f >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> fun t -> t.GetAwaiter().GetResult()

    run (f' >=> identity) = run f'

[<Property>]
let ``RequestHandler.compose associativity law``
    (f: int -> int option)
    (g: int -> int option)
    (h: int -> int option)
    (req: int)
    =
    let f' = f >> Effect.pure'
    let g' = g >> Effect.pure'
    let h' = h >> Effect.pure'
    let interpreter = createInterpreter ()

    let run h =
        req
        |> h
        |> Effect.interpret interpreter
        |> fun t -> t.GetAwaiter().GetResult()

    run ((f' >=> g') >=> h') = run (f' >=> (g' >=> h'))


open RequestMiddleware

[<Fact>]
let ``RequestMiddleware.lift should call the next handler only if the lifted handler returns None`` () =
    task {
        //arrange
        let handler1 = mock (fun (_: SomeRequest) -> effect { return None })
        let handler2 = mock (fun (_: SomeRequest) -> effect { return Some() })
        let next = mock (fun (_: SomeRequest) -> effect { return Some() })

        let requestHandler = lift handler1.Fn <| lift handler2.Fn next.Fn
        let interpreter = createInterpreter ()

        //act
        do!
            { Code = "" }
            |> requestHandler
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handler1 |> wasCalled |> should be True
        handler2 |> wasCalled |> should be True
        next |> wasCalled |> should be False
    }

[<Fact>]
let ``RequestMiddleware.handlers should call each handler until one returns Some`` () =
    task {
        //arrange
        let handle1 = mock (fun (_: SomeRequest) -> effect { return None })
        let handle2 = mock (fun (_: SomeRequest) -> effect { return Some() })
        let handle3 = mock (fun (_: SomeRequest) -> effect { return Some() })

        let requestPipeline = handlers [
            handle1.Fn
            handle2.Fn
            handle3.Fn
        ]

        let interpreter = createInterpreter ()

        //act
        do!
            { Code = "" }
            |> run requestPipeline
            |> Effect.interpret interpreter
            |> Task.ignore

        //assert
        handle1 |> wasCalled |> should be True
        handle2 |> wasCalled |> should be True
    }
