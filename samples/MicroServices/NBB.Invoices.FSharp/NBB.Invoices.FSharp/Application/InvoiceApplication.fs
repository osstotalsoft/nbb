namespace NBB.Invoices.FSharp.Application

open System
open NBB.Invoices.FSharp.Domain
open NBB.Core.Effects.FSharp
open NBB.Core.Evented.FSharp
open NBB.Application.Mediator.FSharp

module InvoiceApplication =
    type Command =
        | CreateInvoice of ClientId: Guid * ContractId: Guid option * Amount: decimal
        | MarkInvoiceAsPayed of InvoiceId: Guid * PaymentId: Guid
        interface ICommand

    let handle cmd =
        match cmd with
        | CreateInvoice (clientId, contractId, amount) ->
            effect {
                let eventedInvoice =
                    InvoiceAggregate.create clientId contractId amount

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
        | MarkInvoiceAsPayed (invoiceId, paymentId) ->
            effect {
                let! invoice = InvoiceRepository.getById invoiceId

                let eventedInvoice =
                    InvoiceAggregate.markAsPayed paymentId invoice

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
