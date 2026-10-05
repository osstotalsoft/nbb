// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Application

open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp
open NBB.Invoices.FSharp.Domain
open InvoiceAggregate

/// Translation between the NBB.Invoices published language (shared with the C# Invoices service and the
/// orchestration process manager) and the F# application, so that this service is a drop-in replacement.
module PublishedLanguage =
    type private CreateInvoiceMessage = NBB.Invoices.PublishedLanguage.CreateInvoice
    type private MarkInvoiceAsPayedMessage = NBB.Invoices.PublishedLanguage.MarkInvoiceAsPayed
    type private ProcessInvoiceMessage = NBB.Invoices.PublishedLanguage.ProcessInvoice
    type private InvoiceCreatedMessage = NBB.Invoices.PublishedLanguage.InvoiceCreated
    type private InvoiceMarkedAsPayedMessage = NBB.Invoices.PublishedLanguage.InvoiceMarkedAsPayed

    /// The published language commands handled by this service
    let commandTypes =
        [ typeof<CreateInvoiceMessage>
          typeof<MarkInvoiceAsPayedMessage>
          typeof<ProcessInvoiceMessage> ]

    /// Sends a message received from the bus to the mediator, translating published language commands
    let handleMessage (message: obj) : Effect<unit> =
        match message with
        | :? CreateInvoiceMessage as cmd ->
            Mediator.sendCommand (
                { ClientId = cmd.ClientId
                  ContractId = Option.ofNullable cmd.ContractId
                  Amount = cmd.Amount }: CreateInvoice.Command
            )
        | :? MarkInvoiceAsPayedMessage as cmd ->
            Mediator.sendCommand (
                { InvoiceId = cmd.InvoiceId
                  PaymentId = cmd.PaymentId }: MarkInvoiceAsPayed.Command
            )
        | :? ProcessInvoiceMessage as cmd ->
            Mediator.sendCommand ({ InvoiceId = cmd.InvoiceId }: ProcessInvoice.Command)
        | _ -> Mediator.sendMessage message

    /// Publishes the integration events for the invoice domain events
    let publishIntegrationEvents: EventHandler<InvoiceEvent> =
        fun event ->
            effect {
                match event with
                | InvoiceCreated invoice ->
                    do!
                        MessageBus.publish (
                            InvoiceCreatedMessage(
                                invoice.Id,
                                invoice.Amount,
                                invoice.ClientId,
                                Option.toNullable invoice.ContractId
                            )
                        )
                | InvoicePayed invoice ->
                    do! MessageBus.publish (InvoiceMarkedAsPayedMessage(invoice.Id, Option.toNullable invoice.ContractId))

                return Some()
            }
