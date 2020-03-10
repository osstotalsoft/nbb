namespace NBB.Core.Effects.FSharp.Data

open NBB.Core.Effects.FSharp
open NBB.Core.FSharp.Data

type ReaderEffect<'s, 't> = 's -> Effect<'t>
module ReaderEffect =
    let run (x: ReaderEffect<'s, 't>) : 's -> Effect<'t> = 
        x 

    let map (f: 't->'u) (m : ReaderEffect<'s, 't>) : ReaderEffect<'s,'u> = 
        m >> Effect.map f

    let bind (f: 't-> ReaderEffect<'s, 'u>) (m : ReaderEffect<'s, 't>) : ReaderEffect<'s, 'u> = 
        fun s -> Effect.bind (fun a -> run (f a) s) (run m s)

    let apply (f: ReaderEffect<'s, ('t -> 'u)>) (m: ReaderEffect<'s, 't>) : ReaderEffect<'s, 'u> = 
        fun s -> Effect.bind (fun g -> Effect.map (fun (a: 't) -> (g a)) (run m s)) (f s)

    let pure' x = 
        fun _ -> Effect.pure' x

    let lift (eff : Effect<'t>) : ReaderEffect<'s, 't> =
        fun _ -> eff

    let hoist (reader : Reader<'s, 't>) : ReaderEffect<'s, 't> =
        fun s -> Effect.pure' (reader s)

module ReaderEffectBulder =
    type ReaderEffectBulder() =
        member _.Bind (m, f) = ReaderEffect.bind f m                    : ReaderEffect<'s,'u>
        member _.Return x = ReaderEffect.pure' x                        : ReaderEffect<'s,'u>
        member _.ReturnFrom x = x                                       : ReaderEffect<'s,'u>
        member _.Combine (m1, m2) = ReaderEffect.bind (fun _ -> m1) m2  : ReaderEffect<'s,'u>
        member _.Zero () = ReaderEffect.pure' ()                        : ReaderEffect<'s, unit>

[<AutoOpen>]
module ReaderEffectExtensions =
    let rde = new ReaderEffectBulder.ReaderEffectBulder()

    let (<!>) = ReaderEffect.map
    let (<*>) = ReaderEffect.apply
    let (>>=) eff func = ReaderEffect.bind func eff


    [<RequireQualifiedAccess>]
    module List =
        let traverseReaderEffect f list =
            let pure' = ReaderEffect.pure'
            let (<*>) = ReaderEffect.apply
            let cons head tail = head :: tail  
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceReaderEffect list = traverseReaderEffect id list

    [<RequireQualifiedAccess>]
    module Result = 
          let traverseReaderEffect (f: 'a-> ReaderEffect<'s, 'b>) (result:Result<'a,'e>) : ReaderEffect<'s, Result<'b, 'e>> = 
              match result with
                  |Error err -> ReaderEffect.pure' (Error err)
                  |Ok v -> ReaderEffect.map Result.Ok (f v)

          let sequenceReaderEffect result = traverseReaderEffect id result