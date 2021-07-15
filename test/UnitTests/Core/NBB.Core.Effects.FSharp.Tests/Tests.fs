module Tests
open Xunit
open FsUnit.Xunit
open NBB.Core.Effects.FSharp
open NBB.Core.Effects.FSharp.Interpreter

[<Fact>]
let ``Pure(1) + Pure(2) should equal Pure(3)`` () =
    let interpreter = createInterpreter()
    let eff = 
        Strict.effect {
            let! x = Effect.pure' 1
            let! y = Effect.pure' 2
            return x + y
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal 3

[<Fact>]
let ``Lazy effect with try/with expression catches exception`` () =
    let interpreter = createInterpreter()
    let eff = 
        effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok (x/0)
            with
                e -> return Result.Error(e.Message)
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,string>.Error((System.DivideByZeroException()).Message))

[<Fact>]
let ``Strict effect with try/with expression catches exception`` () =
    let interpreter = createInterpreter()
    let eff = 
        Strict.effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok (x/0)
            with
                e -> return Result.Error(e.Message)
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,string>.Error((System.DivideByZeroException()).Message))


[<Fact>]
let ``Lazy effect with try/finally expresion`` () =
    let interpreter = createInterpreter()
    let mutable finallyCalled = false
    let eff = 
        effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok x
            finally
                finallyCalled <- true
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,obj>.Ok 1)

    finallyCalled |> should equal true

[<Fact>]
let ``Strict effect with try/finally expresion`` () =
    let interpreter = createInterpreter()
    let mutable finallyCalled = false
    let eff = 
        Strict.effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok x
            finally
                finallyCalled <- true
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,obj>.Ok 1)

    finallyCalled |> should equal true


[<Fact>]
let ``Lazy effect with try/with block success branch`` () =
    let interpreter = createInterpreter()
    let eff = 
        effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok x
            with
                e -> return Result.Error(e.Message)
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,string>.Ok(1))

[<Fact>]
let ``Strict effect with try/with block success branch`` () =
    let interpreter = createInterpreter()
    let eff = 
        Strict.effect {
            try 
                let! x = Effect.pure' 1
                return Result.Ok x
            with
                e -> return Result.Error(e.Message)
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,string>.Ok(1))

[<Fact>]
let ``Lazy effect with using block`` () =
    let interpreter = createInterpreter()
    let mutable disposeCalled = false
    let resource = {
        new System.IDisposable with
            member _.Dispose() = disposeCalled <- true
    }
        
    let eff = 
        effect {
            use! _disposable = Effect.pure' resource
            return Result.Ok 1
        }
        
    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,obj>.Ok(1))
        
    disposeCalled |> should equal true

[<Fact>]
let ``Strict effect with using block`` () =
    let interpreter = createInterpreter()
    let mutable disposeCalled = false
    let resource = {
        new System.IDisposable with
            member _.Dispose() = disposeCalled <- true
    }

    let eff = 
        Strict.effect {
            use! _disposable = Effect.pure' resource
            return Result.Ok 1
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal (Result<int,obj>.Ok(1))

    disposeCalled |> should equal true

[<Fact>]
let ``Effect computations are lazy evaluated`` () =
    let interpreter = createInterpreter()
    let mutable printfnWasCalled = false
    let printfn _str = 
        printfnWasCalled <- true

    let eff = 
        effect {
            printfn "Execute side effect"
        }

    eff |> ignore
    printfnWasCalled |> should equal false
        
    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal ()

    printfnWasCalled |> should equal true

[<Fact>]
let ``Sequenced effect computations`` () =
    let mapper crt =    
        Strict.effect {
            let! x = Effect.from (fun _ -> 1)
            let! y = Effect.from (fun _ -> 2)
            return x + y + crt
        }
    let eff = [1..5000] |> List.traverse mapper

    use interpreter = createInterpreter()

    let result = 
        eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously

    result.Length |> should equal 5000

[<Fact>]
let ``Sequenced effect computations with CE`` () =

    let eff = 
        effect {
            for x in effect { return [1..5000] } do
            yield x+1
        }

    use interpreter = createInterpreter()

    let result = 
        eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously

    result.Length |> should equal 5000