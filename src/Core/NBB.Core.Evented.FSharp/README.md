# NBB.Core.Evented.FSharp

F# writter monad for evented computations

## NuGet install
```
dotnet add package NBB.Core.Evented.FSharp
```

## Evented values
An evented value is a product of some value as the payload and a list of events
```fsharp
type Evented<'a, 'e> = Evented of payload:'a * events:'e list
```

## Haskell style custom operators
```fsharp
let (<!>) = Evented.map
let (<*>) = Evented.apply
let (>>=) evented func = Evented.bind func evented
let (>=>) = Evented.composeK
```

## Monadic computation expression
Inside evented ces you can accumulate events in an imperative fashion with the addEvent function
```fsharp
let create (id,amount) =
    evented{
        let payment = {Id=id; Amount=amount; Cancelled=false}
        do! addEvent <| Added(id,amount)
        return payment
    }
```

## Evented domain sample
In the sample below we model some evented domain using the evented monad
```fsharp
module Sample =
    type Payment = {
        Id: int
        Amount: decimal
        Cancelled: bool
    }

    type DomainEvent = 
        | Added of Id:int * Amount:decimal
        | Cancelled of Id:int

    let create (id,amount) = 
        evented{
            let payment = {Id=id; Amount=amount; Cancelled=false}
            do! addEvent <| Added(id,amount)
            return payment
        }

    let cancell payment = 
        evented{
            let payment' = {payment with Cancelled=true}
            do! addEvent <| Cancelled payment.Id
            return payment'
        }

    let createCancelled = create >=> cancell
```




