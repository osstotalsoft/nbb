// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Core.FSharp.Data.Reader

type Reader<'s, 't> = 's -> 't
module Reader =
    let run (x: Reader<'s, 't>) : 's -> 't =
        x

    let map (f: 't->'u) (m : Reader<'s, 't>) : Reader<'s,'u> =
        m >> f

    let bind (f: 't-> Reader<'s, 'u>) (m : Reader<'s, 't>) : Reader<'s, 'u> =
        fun s -> let a = run m s in run (f a) s

    let apply (f: Reader<'s, ('t -> 'u)>) (m: Reader<'s, 't>) : Reader<'s, 'u> =
        fun s -> let f = run f s in let a = run m s in f a

    let pure' x =
        fun _ -> x

    let composeK f g x = bind g (f x)

    let ask : Reader<'s, 's> =
        id




module ReaderBulder =
    type ReaderBulder() =
        member _.Bind (m, f) = Reader.bind f m                    : Reader<'s,'u>
        member _.Return x = Reader.pure' x                        : Reader<'s,'u>
        member _.ReturnFrom x = x                                 : Reader<'s,'u>
        member _.Combine (m1, m2) = Reader.bind (fun _ -> m1) m2  : Reader<'s,'u>
        member _.Zero () = Reader.pure' ()                        : Reader<'s, unit>

[<AutoOpen>]
module ReaderExtensions =
    let reader = new ReaderBulder.ReaderBulder()

    let (<!>) = Reader.map
    let (<*>) = Reader.apply
    let (>>=) st func = Reader.bind func st
    let (>=>) = Reader.composeK


    [<RequireQualifiedAccess>]
    module List =
        let traverseReader f list =
            let pure' = Reader.pure'
            let (<*>) = Reader.apply
            let cons head tail = head :: tail
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceReader list = traverseReader id list

    [<RequireQualifiedAccess>]
    module Result =
        let traverseReader (f: 'a-> Reader<'s, 'b>) (result:Result<'a,'e>) : Reader<'s, Result<'b, 'e>> =
            match result with
                | Error err -> Reader.pure' (Error err)
                | Ok v -> Reader.map Result.Ok (f v)

        let sequenceReader result = traverseReader id result
