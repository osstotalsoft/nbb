module Application

open NBB.Core.Effects.FSharp
open NBB.Core.Abstractions
open NBB.Application.Mediator.FSharp
open System

type Command1(code) =
    interface ICommand
    member _.Code with get() : string = code

type Command2(code) =
    interface ICommand     
    member _.Code with get() : string = code

let handle1 (command: Command1) = effect { return Some () }
let handle2 (command: Command2) = effect { return Some () }

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

//let dispatchCommand: CommandHandler<ICommand> =  
//    fun command -> 
//        match command with
//            | :? Command1 -> handle1 (command:?> Command1)
//            | :? Command2 -> handle2 (command:?> Command2)
//            | _ -> effect{ failwith "Invalid command" }


            

module WriteApplication = 
    //let commandHandler: CommandHandler<ICommand> = 
    //    handleCommandExceptions 
    //    << logCommand 
    //    << logCommand 
    //    <| dispatchCommand
    let commandPipeline = 
        handleCommandExceptions
        << logCommand
        << choose [
            handle1 |> liftCommandHandler |> When typeof<Command1>
            logCommand << (handle2 |> liftCommandHandler) |> When typeof<Command2>
            choose [
                logCommand << (handle2 |> liftCommandHandler) |> When typeof<Command2>
            ] |> When typeof<Command2>
        ]

    let sendCommand (cmd: 'TCommand) = runCommand commandPipeline cmd
     


