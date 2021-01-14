namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp

type IQuery = interface end
type IQuery<'TResponse> = 
    inherit IQuery

type QueryHandler<'TQuery, 'TResponse when 'TQuery :> IQuery> = RequestHandler<'TQuery, 'TResponse>
type QueryHandler = QueryHandler<IQuery, obj>
type QueryMiddleware<'TQuery, 'TResponse when 'TQuery :> IQuery> = RequestMiddleware<'TQuery, 'TResponse>
type QueryMiddleware = QueryMiddleware<IQuery, obj>

module QueryHandler =
    let upCast (queryHandler: QueryHandler<'TQuery, 'TResponse>) : QueryHandler = 
        fun query ->
            match query with
            | :? 'TQuery as query' -> query' |> queryHandler |> (Effect.map << Option.map) box
            | _ -> Effect.pure' None

module QueryMidleware =
    let run (middleware: QueryMiddleware) (query: 'TQuery when 'TQuery :> IQuery<'TResponse>) = 
        query :> IQuery |> RequestMiddleware.run middleware |> Effect.map (Option.map (fun x -> x :?> 'TResponse))

