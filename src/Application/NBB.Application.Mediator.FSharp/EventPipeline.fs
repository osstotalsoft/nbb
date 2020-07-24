namespace NBB.Application.Mediator.FSharp

open NBB.Core.Abstractions
open NBB.Core.Effects.FSharp


type EventHandler<'TEvent when 'TEvent :> IEvent> = 'TEvent -> Effect<unit option>
type EventMiddleware<'TEvent when 'TEvent :> IEvent> = EventHandler<'TEvent> -> EventHandler<'TEvent>

