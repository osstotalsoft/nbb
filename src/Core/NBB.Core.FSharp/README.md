# NBB.Core.FSharp

F# reader writter state monadic computations

## NuGet install
```
dotnet add package NBB.Core.FSharp
```

## Reader computations
A reader value is a computation that depends on s shared environment. See the example usage of reader computation expression below.

```fsharp
module ReaderSample

open NBB.Core.FSharp.Data.Reader

let add x = 
    reader {
        let! y = Reader.ask
        return x + y
    }

let mult x = 
    reader {
        let! y = Reader.ask
        return x * y
    }

let addThenMult = add >=> mult
let result = Reader.run (addThenMult 2) 3
```

## Statefull computations
A statefull computation has the effect of modifying the state besides returning the value. See the example usage of state computation expression below.

```fsharp
open NBB.Core.FSharp.Data.State

let inc x = x + 1

let add x = 
    state {
        let! s = State.get
        do! State.modify inc
        return x + s
    }

let mult x = 
    state {
        let! s = State.get
        do! State.modify inc
        return x * s
    }

let addThenMult = add >=> mult
let (result, state) = State.run (addThenMult 2) 3
```

## Reader state computations
A reader state computational effect can read a value from a shared environment and also modify some state

```fsharp
open NBB.Core.FSharp.Data.ReaderState

let inc x = x + 1

let add x = 
    readerState {
        let! s = ReaderState.ask
        do! ReaderState.modify inc
        return x + s
    }

let mult x = 
    readerState {
        let! s = ReaderState.ask
        do! ReaderState.modify inc
        return x * s
    }

let addThenMult = add >=> mult
let (result, state) = ReaderState.run (addThenMult 2) 3 4
```