namespace NBB.Invoices.FSharp.Application

open System
open NBB.Invoices.FSharp.Domain
open NBB.Core.Effects.FSharp
open NBB.Core.Evented.FSharp
open NBB.Application.Mediator.FSharp

module CreateInvoice =
    type Command =
        { ClientId: Guid
          ContractId: Guid
          Amount: decimal }
        interface ICommand

    let handle cmd =
        effect {
            let eventedInvoice =
                InvoiceAggregate.create cmd.ClientId (Some cmd.ContractId) cmd.Amount

            do!
                eventedInvoice
                |> Evented.run
                |> fst
                |> InvoiceRepository.save

            do!
                eventedInvoice
                |> Evented.exec
                |> Mediator.dispatchEvents

            return Some()
        }

module MarkInvoiceAsPayed =
    type Command =
        { InvoiceId: Guid
          PaymentId: Guid }
        interface ICommand

    let handle cmd =
        effect {
            let! invoice = InvoiceRepository.getById cmd.InvoiceId

            let eventedInvoice =
                InvoiceAggregate.markAsPayed cmd.PaymentId invoice

            do!
                eventedInvoice
                |> Evented.run
                |> fst
                |> InvoiceRepository.save

            do!
                eventedInvoice
                |> Evented.exec
                |> Mediator.dispatchEvents

            return Some()
        }
