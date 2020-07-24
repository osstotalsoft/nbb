module Application

open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
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
    interface IQuery<bool> with
        member _.GetResponseType(): Type = typeof<bool>

module Query1 =
    let handle (q: Query1) =
        effect {
            return Some true
        }

type Query2 = 
    { Code: string }
    interface IQuery<bool> with
        member _.GetResponseType(): Type = typeof<bool>

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


module WriteApplication = 
    let private commandPipeline = 
        handleExceptions //generic middleware
        << logRequest //generic middleware
        << lift validateTenantCmd//generic validator
        << choose [
            Command1.handle |> lift |> RequestMiddleware.cast
        ]
        << handlers [
            Command1.validate' >=> Command1.handle |> CommandHandler.upCast//just handler
            lift Command2.validate Command2.handle |> CommandHandler.upCast //handler + validator
            Command3.handle1 >=> Command3.handle2 |> CommandHandler.upCast //handler composition
        ]

    let sendCommand (cmd: 'TCommand) = CommandMiddleware.run commandPipeline cmd

module ReadApplication = 
    let private queryPipeline = 
        handleExceptions
        << logRequest //generic validator
        << lift validateTenantQuery //generic validator
        << handlers [
            Query1.handle |> QueryHandler.upCast
            lift Query2.validate Query2.handle |> QueryHandler.upCast
        ]

    let sendQuery (query: 'TQuery) = QueryMidleware.run queryPipeline query




//call site
let cmdEff = WriteApplication.sendCommand { Command1.Code = "code" }
let queryEff = ReadApplication.sendQuery { Query1.Code = "code" }
     


