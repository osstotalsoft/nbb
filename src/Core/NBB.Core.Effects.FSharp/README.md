# NBB.Core.Effects.FSharp

F# functions and computation expressions for NBB.Core.Effects

## NuGet install
```
dotnet add package NBB.Core.Effects.FSharp
```

## Effect computation expressions
Lets you compose effects the fsharp way.
There is one, more efficient, strict computation expression *Strict.effect* and a lazy one *Lazy.effect'*

### Lazy computation expression effect
The default effect computation expression is `Lazy.effect`. The `Lazy` module import/qualification can be ommited since it is automatically opened.

```fsharp
open NBB.Core.Effects.FSharp

let handler (IncrementCommand id) = 
    effect {
        let! agg = loadById id
        let agg' = agg |> increment
        do! save agg'
    }
```

### Strict computation expression effect
To use the strict version of the computation expression you can open the `NBB.Core.Effects.FSharp.Strict` module, or qualify the computation expression (`Strict.effect`)

```fsharp
NBB.Core.Effects.FSharp.Strict

let handler (IncrementCommand id) = 
    effect {
        let! agg = loadById id
        let agg' = agg |> increment
        do! save agg'
    }
```


## Lifting side-effects into effects
Use *Effect.Of* function to lift side-effects into effects
```fsharp
module ConsoleEffects

open NBB.Core.Effects

type ConsoleSideEffect<'a> =
    | ReadLine of (string -> 'a)
    | WriteLine of str: string * (unit -> 'a)
    interface ISideEffect<'a>

let readLine : Effect<string> =
    Effect.Of(ReadLine id)

let writeLine str : Effect<unit> = 
    Effect.Of(WriteLine (str id))
```

## Haskell style custom operators
Where possible we have created Haskell style operators
```fsharp
let (<!>) = Effect.map
let (<*>) = Effect.apply
let (>>=) eff func = Effect.bind func eff
let (>=>) = Effect.composeK
```


## Custom effects sample
In the sample below we define some custom console side-effects, we lift them in the Effect type, and we combine this effects in various ways, using the effect computation expression and functions 
```fsharp
module ConsoleEffects

open NBB.Core.Effects

type ConsoleSideEffect<'a> =
    | ReadLine of (string -> 'a)
    | WriteLine of str: string * (unit -> 'a)
    interface ISideEffect<'a>

let readLine : Effect<string> =
    Effect.Of(ReadLine id)

let writeLine str : Effect<unit> = 
    Effect.Of(WriteLine (str id))


let handle (sideEffect : ConsoleSideEffect<'a>) () = 
    match sideEffect with
    | ReadLine continuation ->
        let line = System.Console.ReadLine()
        continuation line
    | WriteLine (str, continuation) ->
        System.Console.WriteLine str
        continuation ()


module Program

open NBB.Core.Effects.FSharp

module Console =
    let writeLine =
        Effects.Console.WriteLine >> Effect.ignore

    let readLine = Effects.Console.ReadLine

let fizzBuzz n =
    match n with
    | value when (value % 3 = 0 && value % 5 = 0) -> "FizzBuzz"
    | value when (value % 3 = 0) -> "Fizz"
    | value when (value % 5 = 0) -> "Buzz"
    | _ -> "No Fizz No Buzz"


let main =
    effect {
        do! Console.writeLine "Give me a number"
        let! str = Console.readLine
        let fb = str |> System.Int32.Parse |> fizzBuzz
        do! Console.writeLine fb
    }

let main1 =
    effect' {
        do System.Console.WriteLine "Give me a number"
        let str = System.Console.ReadLine()
        let fb = str |> System.Int32.Parse |> fizzBuzz
        do System.Console.WriteLine fb
    }

let main2 =
    Console.writeLine "Give me a number"
    >>= fun _ ->
        Console.readLine
        >>= fun str ->
                let n = str |> System.Int32.Parse |> fizzBuzz
                Console.writeLine n

let main3 =
    [ Console.writeLine "Give me a number"
      System.Int32.Parse >> fizzBuzz
      <!> Console.readLine
      >>= Console.writeLine ]
    |> List.sequence_

let main4 =
    Console.writeLine "Give me a number"
    |> Effect.bind
        (fun _ ->
            Console.readLine
            |> Effect.map (System.Int32.Parse >> fizzBuzz)
            |> Effect.bind Console.writeLine)

let main5 =
    [ "Hello"; "World" ]
    |> List.traverse_ Console.writeLine
```




