module Application

open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open NBB.Application.Mediator.FSharp
open System

type Command1(code) =
    interface ICommand
    member _.Code with get() : string = code

let handle1 (command: Command1) = effect { return Some () }
let validate1 (command: Command1) = 
    effect {
        if command.Code = ""
        then 
            failwith "Empty code" |> ignore
            return None
        else
            return Some command
    }


type Command2(id) =
    interface ICommand     
    member _.Id with get() : string = id


let handle2 (command: Command2) = effect { return Some () }
let validate2 (command: Command2) = 
    effect {
        return Some ()
    }

let logCommand: CommandMiddleware = 
    fun next command ->
        effect {
            Console.WriteLine "before"
            let! result = next command
            Console.WriteLine "after"
            return result
        }

let handleCommandExceptions: CommandMiddleware =  
    fun next command ->
        effect {
            Console.WriteLine "try"
            let! result = next command
            Console.WriteLine "finally"
            return result
        }


module WriteApplication = 
    let commandPipeline = 
        handleCommandExceptions
        << logCommand
        << handlers [
            validate1 >=> handle1 |> CommandHandler.upCast
            handle2 |> CommandHandler.upCast
        ]

    let sendCommand (cmd: 'TCommand) = CommandMiddleware.run commandPipeline cmd
    let eff = sendCommand (Command1 "code")
     


