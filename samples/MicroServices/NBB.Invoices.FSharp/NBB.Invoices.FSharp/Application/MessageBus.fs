// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Application

open NBB.Core.Effects.FSharp

// TODO: Find a place for MesageBus wrapper
module MessageBus =
    let publish (obj: 'TMessage) =
        NBB.Messaging.Effects.MessageBus.Publish(obj :> obj)
        |> Effect.ignore

