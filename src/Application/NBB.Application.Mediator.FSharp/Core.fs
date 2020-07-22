[<AutoOpen>]
module NBB.Application.Mediator.FSharp.Core

open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open System

type EventHandler<'TEvent when 'TEvent :> IEvent> = 'TEvent -> Effect<unit option>
type EventMiddleware<'TEvent when 'TEvent :> IEvent> = EventHandler<'TEvent> -> EventHandler<'TEvent>

type RequestHandler<'TRequest, 'TResponse> = 'TRequest -> Effect<'TResponse option>
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

    let empty : RequestHandler<'TRequest, 'TResponse> = 
        fun _ -> Effect.pure' None


type RequestMiddleware<'TRequest, 'TResponse> = RequestHandler<'TRequest, 'TResponse> -> RequestHandler<'TRequest, 'TResponse>
type CommandMiddleware = RequestMiddleware<ICommand, unit>
type QueryMiddleware = RequestMiddleware<IQuery, obj>

let lift (handler: RequestHandler<'TRequest, 'TResponse>) : RequestMiddleware<'TRequest, 'TResponse> =
    fun (next: RequestHandler<'TRequest, 'TResponse>) ->
        fun (request: 'TRequest) ->
            effect{
                let! result = handler request
                match result with
                | Some respone -> return Some respone
                | None -> return! next request
            }


let liftCommandHandler (handler: RequestHandler<'TCommand, unit>) : CommandMiddleware = 
    let handler': RequestHandler<ICommand, unit> = 
        fun req ->
            match req with
            | :? 'TCommand as req' -> handler req'
            | _  -> Effect.pure' None

    handler' |> lift

let mappend (m1: RequestMiddleware<'TRequest, 'TResponse>) (m2: RequestMiddleware<'TRequest, 'TResponse>) : RequestMiddleware<'TRequest, 'TResponse> =
    m1 >> m2

let mapRequest (mapping1: 'TRequest -> 'TOtherRequest) (mapping2: 'TOtherRequest -> 'TRequest) (middleware: RequestMiddleware<'TRequest, 'TResponse>) : RequestMiddleware<'TOtherRequest, 'TResponse> =
    fun (next : RequestHandler<'TOtherRequest, 'TResponse>) ->
        middleware (mapping1 >> next) |> RequestHandler.contramap mapping2

//let upcastRequest<'TRequest, 'TSuperRequest when 'TRequest :> 'TSuperRequest> (middleware: RequestMiddleware<'TRequest, 'TResponse>) : RequestMiddleware<'TSuperRequest, 'TResponse> = 
//    mapRequest (fun (req: 'TRequest) -> req :> 'TSuperRequest) (fun (req: 'TSuperRequest) -> req :?> 'TRequest) middleware

let choose (middlewares: RequestMiddleware<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
    fun next ->
        let handlers = middlewares |> List.map (fun h -> h next)
        fun req ->
            RequestHandler.choose handlers req

let cond (pred: 'TRequest -> bool) (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TRequest, 'TResponse> =
    fun next req ->
        match pred req with
        | true -> middleware next req
        | false -> Effect.pure' None

let When (requestType: Type) (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TRequest, 'TResponse> =
    let pred (req: 'TRequest): bool =  requestType.IsAssignableFrom(req.GetType())

    cond pred middleware
    

let run (middleware: RequestMiddleware<'TRequest, 'TResponse>) = middleware RequestHandler.empty

let runCommand (middleware: CommandMiddleware) (cmd: 'TCommand when 'TCommand :> ICommand) = cmd :> ICommand |> run middleware

let runQuery (middleware: QueryMiddleware) (query: 'TQuery when 'TQuery :> IQuery<'TResponse>) = 
    query :> IQuery |> run middleware |> Effect.map (Option.map (fun x -> x :?> 'TResponse))












