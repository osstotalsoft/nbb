[<AutoOpen>]
module NBB.Application.Mediator.FSharp.Core

open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open System

type EventHandler<'TEvent when 'TEvent :> IEvent> = 'TEvent -> Effect<unit option>
type EventMiddleware<'TEvent when 'TEvent :> IEvent> = EventHandler<'TEvent> -> EventHandler<'TEvent>
type RequestHandler<'TRequest, 'TResponse> = 'TRequest -> Effect<'TResponse option>
type CommandHandler<'TCommand when 'TCommand :> ICommand> = RequestHandler<'TCommand, unit>
type RequestMiddleware<'TRequest, 'TResponse> = RequestHandler<'TRequest, 'TResponse> -> RequestHandler<'TRequest, 'TResponse>
type CommandMiddleware = RequestMiddleware<ICommand, unit>
type QueryMiddleware = RequestMiddleware<IQuery, obj>


module Handler = 
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


module CommandHandler =
    let upCast (commandHandler: CommandHandler<'TCommand>) : CommandHandler<ICommand> = 
        fun cmd ->
            match cmd with
            | :? 'TCommand as cmd' -> commandHandler cmd'
            | _ -> Effect.pure' None


module Middleware =
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
            middleware (mapping1 >> next) |> Handler.contramap mapping2

    let choose (middlewares: RequestMiddleware<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        fun next ->
            let handlers = middlewares |> List.map (fun h -> h next)
            fun req ->
                Handler.choose handlers req

    let handlers (hs: RequestHandler<'TRequest, 'TResponse> list): RequestMiddleware<'TRequest, 'TResponse> =
        Handler.choose hs |> lift

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
            let next' = Handler.castRequest next
            let h = middleware next'
            Handler.castRequest h

    let cast (middleware: RequestMiddleware<'TRequest, 'TResponse>): RequestMiddleware<'TOtherRequest, 'TOtherResponse> =
        fun next ->
            let next' = Handler.cast next
            let h = middleware next'
            Handler.cast h

    let liftCast (handler:RequestHandler<'TRequest, 'TResponse>): RequestMiddleware<'TOtherRequest, 'TOtherResponse> = 
        handler |> lift |> cast


    let run (middleware: RequestMiddleware<'TRequest, 'TResponse>) = middleware Handler.empty


module CommandMiddleware =
    let run (middleware: CommandMiddleware) (cmd: 'TCommand when 'TCommand :> ICommand) = cmd :> ICommand |> Middleware.run middleware

module QueryMidleware =
    let run (middleware: QueryMiddleware) (query: 'TQuery when 'TQuery :> IQuery<'TResponse>) = 
        query :> IQuery |> Middleware.run middleware |> Effect.map (Option.map (fun x -> x :?> 'TResponse))

[<AutoOpen>]
module Core =
    let lift = Middleware.lift
    let choose = Middleware.choose
    let handlers = Middleware.handlers
    let (>=>) = Handler.compose


    












