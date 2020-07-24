
namespace NBB.Application.Mediator.FSharp

open NBB.Core.Abstractions
open NBB.Core.Effects.FSharp

type CommandHandler<'TCommand when 'TCommand :> ICommand> = RequestHandler<'TCommand, unit>
type CommandMiddleware = RequestMiddleware<ICommand, unit>

module CommandHandler =
    let upCast (commandHandler: CommandHandler<'TCommand>) : CommandHandler<ICommand> = 
        fun cmd ->
            match cmd with
            | :? 'TCommand as cmd' -> commandHandler cmd'
            | _ -> Effect.pure' None

module CommandMiddleware =
    let run (middleware: CommandMiddleware) (cmd: 'TCommand when 'TCommand :> ICommand) = cmd :> ICommand |> RequestMiddleware.run middleware

