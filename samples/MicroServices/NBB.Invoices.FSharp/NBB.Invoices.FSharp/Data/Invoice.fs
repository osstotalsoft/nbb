namespace NBB.Invoices.FSharp.Data

open System
open NBB.Invoices.FSharp.Domain
open InvoiceAggregate

module InvoiceRepoImpl =
    let handle<'a> : InvoiceRepository.SideEffect<'a> -> 'a =
        function
        | InvoiceRepository.GetById (invoiceId, continuation) ->
            { Id = invoiceId
              ClientId = Guid.NewGuid()
              ContractId = Guid.NewGuid() |> Some
              Amount = 100m
              PaymentId = None }
            |> continuation
        | InvoiceRepository.Save (invoice, continuation) -> continuation ()
