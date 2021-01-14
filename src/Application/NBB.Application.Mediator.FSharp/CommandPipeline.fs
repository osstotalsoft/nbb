
namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp

type ICommand = interface end

type CommandHandler<'TCommand when 'TCommand :> ICommand> = RequestHandler<'TCommand, unit>
type CommandHandler = CommandHandler<ICommand>
type CommandMiddleware<'TCommand when 'TCommand :> ICommand> = RequestMiddleware<'TCommand, unit>
type CommandMiddleware = CommandMiddleware<ICommand>

module CommandHandler =
    let upCast (commandHandler: CommandHandler<'TCommand>) : CommandHandler = 
        fun cmd ->
            match cmd with
            | :? 'TCommand as cmd' -> commandHandler cmd'
            | _ -> Effect.pure' None

module CommandMiddleware =
    let run (middleware: CommandMiddleware) (cmd: 'TCommand when 'TCommand :> ICommand) = cmd :> ICommand |> RequestMiddleware.run middleware

