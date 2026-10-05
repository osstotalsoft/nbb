// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Application

open System
open NBB.Core.Effects
open NBB.Core.Effects.FSharp
open NBB.Application.Mediator.FSharp

/// The invoice as exposed by the API, with the same shape as the C# Invoices API
[<CLIMutable>]
type InvoiceView =
    { InvoiceId: Guid
      ClientId: Guid
      ContractId: Nullable<Guid>
      Amount: decimal
      IsPayed: bool
      PaymentId: Nullable<Guid>
      Version: int }

module InvoiceReadModel =
    type SideEffect<'a> =
        | GetAll of Continuation: (InvoiceView list -> 'a)
        | GetById of InvoiceId: Guid * Continuation: (InvoiceView option -> 'a)
        interface ISideEffect<'a>

    let getAll = Effect.Of(GetAll id)
    let getById invoiceId = Effect.Of(GetById(invoiceId, id))

module GetInvoices =
    type Query =
        | Query
        interface IQuery<InvoiceView list>

    let handle (_: Query) =
        InvoiceReadModel.getAll |> Effect.map Some

module GetInvoice =
    type Query =
        { InvoiceId: Guid }
        interface IQuery<InvoiceView option>

    let handle (query: Query) =
        InvoiceReadModel.getById query.InvoiceId |> Effect.map Some
