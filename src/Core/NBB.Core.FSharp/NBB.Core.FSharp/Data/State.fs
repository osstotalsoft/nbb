namespace NBB.Core.FSharp.Data

type State<'s, 't> = 's -> 't * 's
module State =
    let run (x: State<'s, 't>) : 's -> 't * 's = 
        x 

    let map (f: 't->'u) (m : State<'s, 't>) : State<'s,'u> = 
        fun s -> let (a, s') = run m s in (f a, s')

    let bind (f: 't-> State<'s, 'u>) (m : State<'s, 't>) : State<'s, 'u> = 
        fun s -> let (a, s') = run m s in run (f a) s'

    let apply (f: State<'s, ('t -> 'u)>) (m: State<'s, 't>) : State<'s, 'u> = 
        fun s -> let (f, s') = run f s in let (a, s'') = run m s' in (f a, s'')

    let get () : State<'s, 's> = 
        fun s -> (s, s)   

    let put (x: 's) : State<'s, unit> = 
        fun _ -> ((), x)

    let pure' x = 
        fun s -> (x, s)

module StateBulder =
    type StateBulder() =
        member _.Bind (m, f) = State.bind f m                    : State<'s,'u>
        member _.Return x = State.pure' x                        : State<'s,'u>
        member _.ReturnFrom x = x                                : State<'s,'u>
        member _.Combine (m1, m2) = State.bind (fun _ -> m1) m2  : State<'s,'u>
        member _.Zero () = State.pure' ()                        : State<'s, unit>

[<AutoOpen>]
module StateExtensions =
    let state = new StateBulder.StateBulder()

    let (<!>) = State.map
    let (<*>) = State.apply
    let (>>=) st func = State.bind func st

    [<RequireQualifiedAccess>]
    module List =
        let traverseState f list =
            let pure' = State.pure'
            let (<*>) = State.apply
            let cons head tail = head :: tail  
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceState list = traverseState id list
  
    [<RequireQualifiedAccess>]
    module Result = 
          let traverseState (f: 'a-> State<'s, 'b>) (result:Result<'a,'e>) : State<'s, Result<'b, 'e>> = 
              match result with
                  |Error err -> State.map Result.Error (State.pure' err)
                  |Ok v -> State.map Result.Ok (f v)

          let sequenceState result = traverseState id result
