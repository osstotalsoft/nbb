module Tests

open System
open Xunit
open FsUnit.Xunit
open NBB.Core.Evented.FSharp

type AggRoot = AggRoot of int
type DomainEvent = 
    | Added
    | Updated

let create x =  Evented(AggRoot x, [Added])

let increment (AggRoot x) = Evented(AggRoot(x+1), [Updated])

[<Fact>]
let ``Evented workflow test`` () =
    let (Evented(entity, events)) = 
        evented {
            let! x = create 1
            return! increment x
        }
    
    entity|> should equal (AggRoot 2)
    events|> should equal [Added; Updated]
        
