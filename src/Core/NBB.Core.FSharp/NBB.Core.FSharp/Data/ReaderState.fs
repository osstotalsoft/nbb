namespace NBB.Core.FSharp.Data.ReaderState

open NBB.Core.FSharp.Data
open NBB.Core.FSharp.Data.State
open NBB.Core.FSharp.Data.Reader

type ReaderState<'r, 's, 'a> = 'r -> 's -> ('a * 's)
module ReaderState =
    let run (x: ReaderState<'r, 's, 'a>) : 'r -> 's -> ('a * 's) = 
        x 

    let map (f: 'a -> 'b) (m : ReaderState<'r, 's, 'a>) : ReaderState<'r, 's, 'b> = 
        fun r s -> let (a, s') = run m r s in (f a, s')

    let bind (f: 'a-> ReaderState<'r, 's, 'b>) (m : ReaderState<'r, 's, 'a>) : ReaderState<'r, 's, 'b> = 
        fun r s -> let (a, s') = run m r s in run (f a) r s' 

    let apply (f: ReaderState<'r, 's, ('a -> 'b)>) (m: ReaderState<'r, 's, 'a>) : ReaderState<'r, 's, 'b> =
        fun r s -> let (f', s') = run f r s in let (a, s'') = run m r s' in (f' a, s'')

    let ask () : ReaderState<'r, 's, 'r> = 
        fun r s -> (r, s)

    let get () : ReaderState<'r, 's, 's> = 
        fun _r s -> (s, s)

    let put (s: 's) : ReaderState<'r, 's, unit> = 
        fun _r  _s -> ((), s)

    let modify (f: 's -> 's) : ReaderState<'r, 's, unit> =
        get() |> bind (put << f)

    let join (m: ReaderState<'r, 's, ReaderState<'r, 's, 'a>>) : ReaderState<'r, 's, 'a> = 
        m |> bind id 

    let lift (st : State<'s, 'a>) : ReaderState<'r, 's, 'a> =
        fun _r s -> st s

    let hoist (rd: Reader<'r, 'a>) : ReaderState<'r, 's, 'a> =
        fun r s -> (Reader.run rd r, s)

    let pure' (a:'a) : ReaderState<'r, 's, 'a> = 
        a |> State.pure' |> lift


module ReaderStateBuilder =
    type ReaderStateBulder() =
        member _.Bind (m, f) = ReaderState.bind f m                    : ReaderState<'r, 's,'u>
        member _.Return x = ReaderState.pure' x                        : ReaderState<'r, 's,'u>
        member _.ReturnFrom x = x                                            : ReaderState<'r, 's,'u>
        member _.Combine (m1, m2) = ReaderState.bind (fun _ -> m1) m2  : ReaderState<'r, 's,'u>
        member _.Zero () = ReaderState.pure' ()                        : ReaderState<'r, 's, unit>


[<AutoOpen>]
module ReaderStateExtensions =
    let readerState = new ReaderStateBuilder.ReaderStateBulder()

    let (<!>) = ReaderState.map
    let (<*>) = ReaderState.apply
    let (>>=) eff func = ReaderState.bind func eff

    [<RequireQualifiedAccess>]
    module List =
        let traverseReaderState f list =
            let pure' = ReaderState.pure'
            let (<*>) = ReaderState.apply
            let cons head tail = head :: tail  
            let initState = pure' []
            let folder head tail = pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceReaderState list = traverseReaderState id list

    [<RequireQualifiedAccess>]
    module Result = 
          let traverseReaderState (f: 'a-> ReaderState<'r, 's, 'b>) (result:Result<'a,'e>) : ReaderState<'r, 's, Result<'b, 'e>> = 
              match result with
                  | Error err -> ReaderState.pure' (Error err)
                  | Ok v -> ReaderState.map Result.Ok (f v)

          let sequenceReaderState result = traverseReaderState id result