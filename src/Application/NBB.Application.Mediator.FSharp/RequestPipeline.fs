namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp
open System


type RequestHandler<'TRequest, 'TResponse> = 'TRequest -> Effect<'TResponse option>
type RequestMiddleware<'TRequest, 'TResponse> = RequestHandler<'TRequest, 'TResponse> -> RequestHandler<'TRequest, 'TResponse>

module RequestHandler = 
    let rec choose (handlers : RequestHandler<'i, 'o> list) : RequestHandler<'i, 'o> =
        fun (req : 'i) ->
            effect {
                match handlers with
                | [] -> return None
                | handler :: tail ->
                    let! result = handler req
                    match result with
                    | Some c -> return Some c
                    | None   -> return! choose tail req
            }

    let contramap (mapping: 'TOtherRequest -> 'TRequest) (handler: RequestHandler<'TRequest, 'TResponse>) : RequestHandler<'TOtherRequest, 'TResponse> =
        mapping >> handler

    let castRequest(handler: RequestHandler<'TRequest, 'TResponse>) : RequestHandler<'TOtherRequest, 'TResponse> =
        fun req ->
            match box req with
            | :? 'TRequest as req' -> handler req'
            | _ -> Effect.pure' None

    let castResponse (handler: RequestHandler<'TRequest, 'TResponse>) : RequestHandler<'TRequest, 'TOtherResponse> =
        fun req ->
            let tryCast (x:'TResponse) : 'TOtherResponse option = 
                match box x with
                | :? 'TOtherResponse as resp' -> Some resp'
                | _ -> None
            handler req |> Effect.map (Option.bind tryCast)

    let cast (handler: RequestHandler<'TRequest, 'TResponse>) : RequestHandler<'TOtherRequest, 'TOtherResponse> =
        handler |> castRequest |> castResponse

    let empty : RequestHandler<'TRequest, 'TResponse> = 
        fun _ -> Effect.pure' None

    let compose (h1: RequestHandler<'a, 'b>) (h2: RequestHandler<'b, 'c>): RequestHandler<'a, 'c> =
        fun a ->
            effect {
                let! b = h1 a
                match b with
                | None -> return None
                | Some b' -> return! h2 b'
            }

    let (>=>) = compose

module RequestMiddleware =
    let lift (handler: RequestHandler<'TRequest, 'TResponse>) : RequestMiddleware<'TRequest, 'TResponse> =
        fun (next: RequestHandler<'TRequest, 'TResponse>) ->
            fun (request: 'TRequest) ->
                effect {
                    let! result = handler request
                    match result with
                    | Some respone -> return Some respone
                    | None -> return! next request
                }

    let mapRequest (mapping1: 'TRequest -> 'TOtherRequest) (mapping2: 'TOtherRequest -> 'TRequest) (middleware: RequestMiddleware<'TRequest, 'TResponse>) : RequestMiddleware<'TOtherRequest, 'TResponse> =
        fun (next : RequestHandler<'TOtherRequest, 'TResponse>) ->
            middleware (mapping1 >> next) |> RequestHandler.contramap mapping2

    let choose (middlewares: RequestMiddleware<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        fun next ->
            let handlers = middlewares |> List.map (fun h -> h next)
            fun req ->
                RequestHandler.choose handlers req

    let handlers (hs: RequestHandler<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        RequestHandler.choose hs |> lift

    let cond (pred: 'TRequest -> bool) (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TRequest, 'TResponse> =
        fun next req ->
            match pred req with
            | true -> middleware next req
            | false -> Effect.pure' None

    let When (requestType: Type) (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TRequest, 'TResponse> =
        let pred (req: 'TRequest): bool = requestType.IsAssignableFrom(req.GetType())
        cond pred middleware

    let castRequest (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TOtherRequest, 'TResponse> =
        fun next ->
            let next' = RequestHandler.castRequest next
            let h = middleware next'
            RequestHandler.castRequest h

    let cast (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TOtherRequest, 'TOtherResponse> =
        fun next ->
            let next' = RequestHandler.cast next
            let h = middleware next'
            RequestHandler.cast h

    let liftCast (handler:RequestHandler<'TRequest, 'TResponse>): RequestMiddleware<'TOtherRequest, 'TOtherResponse> = 
        handler |> lift |> cast


    let run (middleware: RequestMiddleware<'TRequest, 'TResponse>) = middleware RequestHandler.empty



    












