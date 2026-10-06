// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Application

open System
open NBB.Invoices.FSharp.Domain
open NBB.Core.Effects.FSharp
open NBB.Core.Evented.FSharp
open NBB.Application.Mediator.FSharp

module CreateInvoice =
    type Command =
        { ClientId: Guid
          ContractId: Guid option
          Amount: decimal }
        interface ICommand

    let handle cmd =
        effect {
            let eventedInvoice =
                InvoiceAggregate.create cmd.ClientId cmd.ContractId cmd.Amount

            do! InvoiceRepository.save eventedInvoice

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
            match! InvoiceRepository.getById cmd.InvoiceId with
            | Some invoice ->
                let eventedInvoice =
                    InvoiceAggregate.markAsPayed cmd.PaymentId invoice

                do! InvoiceRepository.save eventedInvoice

                do!
                    eventedInvoice
                    |> Evented.exec
                    |> Mediator.dispatchEvents
            | None -> ()

            return Some()
        }

module ProcessInvoice =
    type Command =
        { InvoiceId: Guid }
        interface ICommand

    let handle cmd =
        effect {
            match! InvoiceRepository.getById cmd.InvoiceId with
            | Some _ ->
                // some computational heavy stuff
                do! Effect.from (fun () -> Threading.Thread.Sleep 1000)
            | None -> ()

            return Some()
        }
