// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Core.Effects.FSharp.Data.ReaderStateEffect

open NBB.Core.Effects.FSharp
open NBB.Core.FSharp.Data
open NBB.Core.FSharp.Data.State
open NBB.Core.FSharp.Data.Reader
open NBB.Core.FSharp.Data.ReaderState
open NBB.Core.Effects.FSharp.Data.ReaderEffect
open NBB.Core.Effects.FSharp.Data.StateEffect

type ReaderStateEffect<'r, 's, 'a> = 'r -> 's -> Effect<'a * 's>
module ReaderStateEffect =
    let run (x: ReaderStateEffect<'r, 's, 'a>) : 'r -> 's -> Effect<'a * 's> =
        x

    let map (f: 'a -> 'b) (m : ReaderStateEffect<'r, 's, 'a>) : ReaderStateEffect<'r, 's, 'b> =
        fun r s -> run m r s |> Effect.map (fun (a, s') -> (f a, s'))

    let bind (f: 'a-> ReaderStateEffect<'r, 's, 'b>) (m : ReaderStateEffect<'r, 's, 'a>) : ReaderStateEffect<'r, 's, 'b> =
        fun r s -> run m r s |> Effect.bind (fun (a, s') -> run (f a) r s')

    let apply (f: ReaderStateEffect<'r, 's, ('a -> 'b)>) (m: ReaderStateEffect<'r, 's, 'a>) : ReaderStateEffect<'r, 's, 'b> =
        fun r s ->
            effect {
                let! (f', s') = run f r s
                let! (a, s'') = run m r s'
                return (f' a, s'')
            }

    let ask () : ReaderStateEffect<'r, 's, 'r> =
        fun r s -> Effect.pure' (r, s)

    let get () : ReaderStateEffect<'r, 's, 's> =
        fun _r s -> Effect.pure' (s, s)

    let put (s: 's) : ReaderStateEffect<'r, 's, unit> =
        fun _r  _s -> Effect.pure' ((), s)

    let modify (f: 's -> 's) : ReaderStateEffect<'r, 's, unit> =
        get() |> bind (put << f)

    let join (m: ReaderStateEffect<'r, 's, ReaderStateEffect<'r, 's, 'a>>) : ReaderStateEffect<'r, 's, 'a> =
        m |> bind id

    let lift (eff : Effect<'a>) : ReaderStateEffect<'r, 's, 'a> =
        fun _r s -> eff |> Effect.map (fun a -> (a, s))

    let pure' (a:'a) : ReaderStateEffect<'r, 's, 'a> =
        a |> Effect.pure' |> lift


    let hoist (readerState: ReaderState<'r, 's, 'a>) : ReaderStateEffect<'r, 's, 'a> =
        fun r s -> Effect.pure' (ReaderState.run readerState r s)

module StateEffectBulder =
    type ReaderStateEffectBulder() =
        member _.Bind (m, f) = ReaderStateEffect.bind f m                    : ReaderStateEffect<'r, 's,'u>
        member _.Return x = ReaderStateEffect.pure' x                        : ReaderStateEffect<'r, 's,'u>
        member _.ReturnFrom x = x                                            : ReaderStateEffect<'r, 's,'u>
        member _.Combine (m1, m2) = ReaderStateEffect.bind (fun _ -> m1) m2  : ReaderStateEffect<'r, 's,'u>
        member _.Zero () = ReaderStateEffect.pure' ()                        : ReaderStateEffect<'r, 's, unit>


[<AutoOpen>]
module ReaderStateEffectExtensions =
    let readerStateEffect = new StateEffectBulder.ReaderStateEffectBulder()

    let (<!>) = ReaderStateEffect.map
    let (<*>) = ReaderStateEffect.apply
    let (>>=) eff func = ReaderStateEffect.bind func eff

    [<RequireQualifiedAccess>]
    module List =
        let traverseReaderStateEffect f list =
            let pure' = ReaderStateEffect.pure'
            let (<*>) = ReaderStateEffect.apply
            let cons head tail = head :: tail
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceReaderStateEffect list = traverseReaderStateEffect id list

    [<RequireQualifiedAccess>]
    module Result =
          let traverseReaderStateEffect (f: 'a-> ReaderStateEffect<'r, 's, 'b>) (result:Result<'a,'e>) : ReaderStateEffect<'r, 's, Result<'b, 'e>> =
              match result with
                  | Error err -> ReaderStateEffect.pure' (Error err)
                  | Ok v -> ReaderStateEffect.map Result.Ok (f v)

          let sequenceReaderStateEffect result = traverseReaderStateEffect id result

    [<RequireQualifiedAccess>]
    module ReaderStateEffect =
        let addCaching (key: 'k) (readerStateEff: ReaderStateEffect<'r, Map<'k, 'v>, 'v>) : ReaderStateEffect<'r, Map<'k, 'v>, 'v> =
            reader {
                let! stateEff = readerStateEff
                return stateEff |> StateEffect.addCaching key
            }
