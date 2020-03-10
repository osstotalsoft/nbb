namespace NBB.Core.Effects.FSharp.Data

open NBB.Core.Effects.FSharp
open NBB.Core.FSharp.Data

type StateEffect<'s, 't> = 's -> Effect<'t * 's>
module StateEffect =
    let run (x: StateEffect<'s, 't>) : 's -> Effect<'t * 's> = 
        x 

    let map (f: 't->'u) (m : StateEffect<'s, 't>) : StateEffect<'s,'u> = 
        fun s -> Effect.map (fun (a, s') -> (f a, s')) (run m s)

    let bind (f: 't-> StateEffect<'s, 'u>) (m : StateEffect<'s, 't>) : StateEffect<'s, 'u> = 
        fun s -> Effect.bind (fun (a, s') -> run (f a) s') (run m s)

    let apply (f: StateEffect<'s, ('t -> 'u)>) (m: StateEffect<'s, 't>) : StateEffect<'s, 'u> = 
        fun s -> Effect.bind (fun (g, s') -> Effect.map (fun (a: 't, s'': 's) -> ((g a), s'')) (run m s')) (f s)

    let pure' x = 
        fun s -> Effect.pure' (x, s)

    let get () : StateEffect<'s, 's> = 
        fun s -> Effect.pure' (s, s)

    let put (x: 's) : StateEffect<'s, unit> = 
        fun _ -> Effect.pure' ((), x)

    let flatten x = 
        bind id x

    let lift (eff : Effect<'t>) : StateEffect<'s, 't> =
        fun s -> eff |> Effect.map (fun a -> (a, s))

    let hoist (state: State<'s, 't>) : StateEffect<'s, 't> =
        fun s -> Effect.pure' (State.run state s)

module StateEffectBulder =
    type StateEffectBulder() =
        member _.Bind (m, f) = StateEffect.bind f m                    : StateEffect<'s,'u>
        member _.Return x = StateEffect.pure' x                        : StateEffect<'s,'u>
        member _.ReturnFrom x = x                                      : StateEffect<'s,'u>
        member _.Combine (m1, m2) = StateEffect.bind (fun _ -> m1) m2  : StateEffect<'s,'u>
        member _.Zero () = StateEffect.pure' ()                        : StateEffect<'s, unit>

    let stateEffectssss = new StateEffectBulder()

[<AutoOpen>]
module StateEffectExtensions =
    let stateEffect = new StateEffectBulder.StateEffectBulder()

    let (<!>) = StateEffect.map
    let (<*>) = StateEffect.apply
    let (>>=) eff func = StateEffect.bind func eff

    [<RequireQualifiedAccess>]
    module List =
        let traverseStateEffect f list =
            let pure' = StateEffect.pure'
            let (<*>) = StateEffect.apply
            let cons head tail = head :: tail  
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceStateEffect list = traverseStateEffect id list

    [<RequireQualifiedAccess>]
    module Result = 
          let traverseStateEffect (f: 'a-> StateEffect<'s, 'b>) (result:Result<'a,'e>) : StateEffect<'s, Result<'b, 'e>> = 
              match result with
                  | Error err -> StateEffect.map Result.Error (StateEffect.pure' err)
                  | Ok v -> StateEffect.map Result.Ok (f v)

          let sequenceStateEffect result = traverseStateEffect id result