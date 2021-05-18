# NBB.Application.Mediator.FSharp

This package provides a mediator for application requests and events for F# projects

## NuGet install
```
dotnet add package NBB.Application.Mediator.FSharp
```

## Request pipeline
A request pipeline is a composition of functions used for handling certain application requests (use-cases)

### Request handler
A `RequestHandler` is a function that receives the request and returns an effect of an optional result.
The optional result measns that the handler can decide whether to process or not the request.

```fsharp
type RequestHandler<'TRequest, 'TResponse> = 'TRequest -> Effect<'TResponse option>
```
Example:
```fsharp
let publishMessage =
    fun msg ->
    effect {
        do! publishMsg msg
        return Some ()
    }
```
Request handlers compose via Kleisli composition:
```fsharp
let myHandler = Command1.validate >=> Command1.handle
```

### Request middleware
A `RequestMiddleware` is just a function that receives the next request handler and returns a resulting request handler
```fsharp
type RequestMiddleware<'TRequest, 'TResponse> = RequestHandler<'TRequest, 'TResponse> -> RequestHandler<'TRequest, 'TResponse>
```

Example:
```fsharp
let logRequest =
    fun next req ->
    effect {
        Console.WriteLine "before"
        let! result = next req
        Console.WriteLine "after"
        return result
    }
```

Request middlewares compose via simple function composition:
```fsharp
let requestPipeline = handleExceptions << logRequest
```

### Request pipeline combinators
- `RequestMiddleware.handlers` - combines a list of request handlers into a request middleware, that when evaluated, evaluates every handler until one returns Some result.
- `RequestMiddleware.lift` - lifts a request handler into a request middleware
- `RequestMiddleware.run` - transforms a request middleware into a request handler


## Command pipeline
A command pipeline is just a request pipeline, where requests are commands and result is unit, so `CommandHandler` and `CommandMiddleware` are just aliases for `RequestHandler` and `RequestMiddleware`

```fsharp
type ICommand = interface end

type CommandHandler<'TCommand when 'TCommand :> ICommand> = RequestHandler<'TCommand, unit>
type CommandHandler = CommandHandler<ICommand>
type CommandMiddleware<'TCommand when 'TCommand :> ICommand> = RequestMiddleware<'TCommand, unit>
type CommandMiddleware = CommandMiddleware<ICommand>
```
### Command pipeline combinators
 - `CommandHandler.upCast` - converts a `CommandHandler<'TCommand>` to a `CommandHandler`. It is useful in conjuction with `RequestMiddleware.handlers`
```fsharp
let cmdPipeline = 
    handlers [
        Command1.handle |> upCast
        Command2.handle |> upCast
    ]
```

 - `CommandMiddleware.run` - transforms a command middleware into a command handler


## Query pipeline
A query pipeline is just a request pipeline, where requests are querries, so `QueryHandler` and `QueryMiddleware` are just aliases for `RequestHandler` and `RequestMiddleware`

```fsharp
type IQuery = interface end
type IQuery<'TResponse> = 
    inherit IQuery

type QueryHandler<'TQuery, 'TResponse when 'TQuery :> IQuery> = RequestHandler<'TQuery, 'TResponse>
type QueryHandler = QueryHandler<IQuery, obj>
type QueryMiddleware<'TQuery, 'TResponse when 'TQuery :> IQuery> = RequestMiddleware<'TQuery, 'TResponse>
type QueryMiddleware = QueryMiddleware<IQuery, obj>
```

### Query pipeline combinators
 - `QueryHandler.upCast` - converts a `QueryHandler<'TQuery, 'TResponse>` to a `QueryHandler`. It is useful in conjuction with `RequestMiddleware.handlers` combinator
```fsharp
let queryPipeline = 
    handlers [
        Query1.handle |> upCast
        Query2.handle |> upCast
    ]
```

 - `QueryMiddleware.run` - transforms a query middleware into a query handler

## Event pipeline
An event pipeline is a composition of functions used for handling certain application events

### Event handler
An `EventHandler` is a function that may handle some event.
It returns an optional unit, that measns that the handler can decide whether to process or not the event.

```fsharp
type EventHandler<'TEvent when 'TEvent :> IEvent> = 'TEvent -> Effect<unit option>
```
Example:
```fsharp
let handle (_: Event1) =
    effect {
        do! someEff
        return () |> Some
    }
```
Event handlers compose via the append fn or (++) operator, which evaluates the handlers in sequence:

```fsharp
let myEvHandler = Event1.handle1 ++ Event1.handle2
```
### Event middleware
An `EventMiddleware` is just a function that receives the next event handler and returns a resulting event handler.

```fsharp
type EventMiddleware<'TEvent when 'TEvent :> IEvent> = EventHandler<'TEvent> -> EventHandler<'TEvent>
```

Example:
```fsharp
let logEvent: EventMiddleware =
    fun next (ev:IEvent) ->
    effect {
        Console.WriteLine "before"
        let! result = next ev
        Console.WriteLine "after"
        return result
    }
```

Event middlewares compose via simple function composition:
```fsharp
let eventPipeline = handleExceptions << logEvent
```

### Event pipeline combinators
 - `EventHandler.upCast` - converts an `EventHandler<'TEvent>` into an `EventHandler`
 - `EventMiddleware.handlers` - combines a list of event handlers into an event middleware, that when evaluated, evaluates every handler until one returns Some result.
 - `EventMiddleware.lift` - lifts an event handler into an event middleware
 - `EventMiddleware.run` - transforms an event middleware into an event handler

## Example application
```fsharp
module Application

open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp
open System

type Command1 =
    { Code: string }
    interface ICommand
module Command1 =
    let handle (_: Command1) = effect { return Some () }
    let validate (command: Command1) = 
        effect {
            if command.Code = ""
            then 
                return failwith "Empty code" |> Some
            else
                return None
        }

    let validate' (command: Command1) = 
        effect {
            if command.Code = ""
            then 
                failwith "Empty code" |> ignore
                return None
            else
                return Some command
        }


type Command2 =
    { Id: int }
    interface ICommand

module Command2 =
    let handle (_: Command2) = effect { return Some () }

    let validate (_: Command2) = 
        effect {
            return None
        }

type Command3 =
    { Id: int }
    interface ICommand

module Command3 =
    let handle1 (_: Command2) = effect { return Some true }
    let handle2 (_: bool) = effect { return Some () }


type Query1 = 
    { Code: string }
    interface IQuery<bool> 

module Query1 =
    let handle (q: Query1) =
        effect {
            return Some true
        }

type Query2 = 
    { Code: string }
    interface IQuery<bool>

module Query2 =
    let handle (_: Query2) =
        effect {
            return Some true
        }
    
    let validate (q: Query2) = 
        effect {
            if q.Code = ""
            then 
                return failwith "Empty code" |> Some
            else
                return None
        }

type Event1 =
    { Id: int; EventId: Guid }
    interface IEvent
        

module Event1 = 
    let handle (_: Event1) =
        effect {
            return () |> Some
        }


type Event2 =
    { Id: int; EventId: Guid }
    interface IEvent
        
        
module Event2 = 
    let handle1 (_: Event2) =
        effect {
            return () |> Some
        }

    let handle2 (_: Event2) =
        effect {
            return () |> Some
        }
   



let handleExceptions =  
    fun next req ->
        effect {
            Console.WriteLine "try"
            let! result = next req
            Console.WriteLine "finally"
            return result
        }

let validateTenantCmd (_: ICommand) =
    effect {
        if "" = ""
        then 
            return failwith "Empty code" |> Some
        else
            return None
    }

let validateTenantQuery (_: IQuery) =
    effect {
        if "" = ""
        then 
            return failwith "Empty code" |> Some
        else
            return None
    }

let logRequest =
    fun next req ->
    effect {
        Console.WriteLine "before"
        let! result = next req
        Console.WriteLine "after"
        return result
    }

let logEvent: EventMiddleware =
    fun next (ev:IEvent) ->
    effect {
        Console.WriteLine "before"
        let! result = next ev
        Console.WriteLine "after"
        return result
    }

let publishMessage =
    fun _ ->
    effect {
        return Some ()
    }


module WriteApplication = 
    open RequestMiddleware
    open RequestHandler
    open CommandHandler

    let private commandPipeline = 
        handleExceptions //generic middleware
        << logRequest //generic middleware
        << lift validateTenantCmd //generic validator
       
        << handlers [
            Command1.handle |> upCast //just handler
            Command1.validate' >=> Command1.handle |> upCast //handler + validator
            lift Command2.validate Command2.handle |> upCast //handler + validator
            Command3.handle1 >=> Command3.handle2  |> upCast //handler composition
        ]

    open EventMiddleware
    open EventHandler

    let private eventPipeline: EventMiddleware =
        handleExceptions //generic middleware
        << logEvent //generic middleware
        << handlers [
            Event1.handle |> upCast //just handler
            Event2.handle1 ++ Event2.handle2 |> upCast //append two handlers
        ]
        << lift publishMessage //generic handler
        

    let sendCommand (cmd: 'TCommand) = CommandMiddleware.run commandPipeline cmd
    let publishEvent (ev: 'TEvent) = EventMiddleware.run eventPipeline ev

module ReadApplication = 
    open RequestMiddleware
    open QueryHandler

    let private queryPipeline = 
        handleExceptions
        << logRequest //generic validator
        << lift validateTenantQuery //generic validator
        << handlers [
            Query1.handle |> upCast
            lift Query2.validate Query2.handle |> upCast
        ]

    let private commandPipeline = 
        handleExceptions
         << logRequest
         << lift publishMessage

    let sendQuery (query: 'TQuery) = QueryMidleware.run queryPipeline query
    let sendCommand (cmd: 'TCommand) = CommandMiddleware.run commandPipeline cmd




//call site
let cmdEff = WriteApplication.sendCommand { Command1.Code = "code" }
let queryEff = ReadApplication.sendQuery { Query1.Code = "code" }
```

