module StateSample

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

