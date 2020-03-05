module Sample

open NBB.Core.Evented.FSharp
open FSharpPlus

module Domain =
    type AggRoot = AggRoot of int
    type DomainEvent = 
        | Added
        | Updated

    let create x =  Evented(AggRoot x, [Added])

    let update (x:AggRoot) = Evented(x, [Updated])
    let increment (AggRoot x) = AggRoot (x + 1)

    let createAndUpdate x = x |> create >>= update
    let createAndUpdate' = create >> bind update
    let createAndUpdate'' = create >=> update
    let createAndUpdate''' x =
        evented {
            let! x' = create x
            let! x'' = update x'
            return x''
        }

    let createAndUpdate''''  x =
        let (Evented(agg, events)) = create x
        let (Evented(agg', events')) = update agg
        Evented(agg', events @ events')


    let createAndIncrement x = x |> create |> map increment
    let createAndIncrement' = create >> map increment
    let createAndIncrement'' x = increment <!> create x
    let createAndIncrement''' x =
        evented {
            let! x' = create x
            return increment x'
        }

    let liftedSum = lift2 (+)
    let z = liftedSum (Evented(1, [Added])) (Evented(2, [Updated]))

    let createAndIncrementList (lst: _ list) = traverse createAndIncrement lst

