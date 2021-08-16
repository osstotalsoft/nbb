// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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

