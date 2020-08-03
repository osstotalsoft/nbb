namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp


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

    let empty : RequestHandler<'TRequest, 'TResponse> = 
        fun _ -> Effect.pure' None

    let identity : RequestHandler<'TRequest, 'TRequest> =
        fun req -> req |> Some |> Effect.pure'

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

    let choose (middlewares: RequestMiddleware<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        fun next ->
            let handlers = middlewares |> List.map (fun h -> h next)
            fun req ->
                RequestHandler.choose handlers req

    let handlers (hs: RequestHandler<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        RequestHandler.choose hs |> lift

    let run (middleware: RequestMiddleware<'TRequest, 'TResponse>) = middleware RequestHandler.empty



    












