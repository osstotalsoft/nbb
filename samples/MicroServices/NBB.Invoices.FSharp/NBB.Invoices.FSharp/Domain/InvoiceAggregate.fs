// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Domain

open System
open NBB.Core.Evented.FSharp
open NBB.Core.Effects
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp

module InvoiceAggregate =
    type Invoice =
        { Id: Guid
          ClientId: Guid
          ContractId: Guid option
          Amount: decimal
          PaymentId: Guid option }

    type InvoiceEvent =
        | InvoiceCreated of Invoice: Invoice
        | InvoicePayed of Invoice: Invoice
        interface IEvent

    let create clientId contractId amount =
        evented {
            let invoice =
                { Id = Guid.NewGuid()
                  ClientId = clientId
                  ContractId = contractId
                  Amount = amount
                  PaymentId = None }

            do! addEvent (InvoiceCreated invoice)
            return invoice
        }

    let markAsPayed paymentId invoice =
        evented {
            let invoice' =
                { invoice with
                      PaymentId = Some paymentId }

            do! addEvent (InvoicePayed invoice')
            return invoice'
        }

    let createPayed clientId contractId amount paymentId =
        evented {
            let! invoice = create clientId contractId amount
            return! markAsPayed paymentId invoice
        }

module InvoiceRepository =
    open InvoiceAggregate

    type SideEffect<'a> =
        | GetById of InvoiceId: Guid * Continuation: (Invoice -> 'a)
        | Save of Invoice: Invoice * Continuation: (unit -> 'a)
        interface ISideEffect<'a>

    let getById invoiceId = Effect.Of(GetById(invoiceId, id))
    let save invoice = Effect.Of(Save(invoice, id))
