// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module ReaderStateSample

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

