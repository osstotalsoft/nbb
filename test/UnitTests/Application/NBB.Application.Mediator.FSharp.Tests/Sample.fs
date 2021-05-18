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
     


