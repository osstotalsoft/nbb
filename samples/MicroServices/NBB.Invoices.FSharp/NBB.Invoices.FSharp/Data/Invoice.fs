// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Data

open System
open System.Threading
open System.Threading.Tasks
open Microsoft.Data.SqlClient
open NBB.Invoices.FSharp.Domain
open NBB.Invoices.FSharp.Application
open InvoiceAggregate

/// SQL Server persistence on the Invoices table of the NBB_Invoices database (created by NBB.Invoices.Migrations),
/// so that this service shares its data with the C# Invoices service.
module private Sql =
    let dbValue (value: Nullable<'T>) : obj =
        if value.HasValue then box value.Value else box DBNull.Value

    let nullable<'T when 'T: struct and 'T: (new: unit -> 'T) and 'T :> ValueType> (reader: SqlDataReader) (ordinal: int) =
        if reader.IsDBNull ordinal then Nullable<'T>() else Nullable<'T>(reader.GetFieldValue<'T> ordinal)

    let query (connectionString: string) (sql: string) (parameters: (string * obj) list) (read: SqlDataReader -> 'r) (ct: CancellationToken) =
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync ct
            use command = new SqlCommand(sql, connection)
            for name, value in parameters do
                command.Parameters.AddWithValue(name, value) |> ignore
            use! reader = command.ExecuteReaderAsync ct
            let results = Collections.Generic.List<'r>()
            let mutable hasRow = true
            while hasRow do
                let! read' = reader.ReadAsync ct
                hasRow <- read'
                if hasRow then results.Add(read reader)
            return List.ofSeq results
        }

    let execute (connectionString: string) (sql: string) (parameters: (string * obj) list) (ct: CancellationToken) =
        task {
            use connection = new SqlConnection(connectionString)
            do! connection.OpenAsync ct
            use command = new SqlCommand(sql, connection)
            for name, value in parameters do
                command.Parameters.AddWithValue(name, value) |> ignore
            let! _ = command.ExecuteNonQueryAsync ct
            return ()
        }

    let readView (reader: SqlDataReader) =
        let paymentId = nullable<Guid> reader 5
        { InvoiceId = reader.GetGuid 0
          ClientId = reader.GetGuid 1
          ContractId = nullable<Guid> reader 2
          Amount = reader.GetDecimal 3
          Version = reader.GetInt32 4
          PaymentId = paymentId
          IsPayed = paymentId.HasValue }

    let selectViews = "SELECT InvoiceId, ClientId, ContractId, Amount, Version, PaymentId FROM Invoices"

module InvoiceRepoImpl =
    let handle<'a> (connectionString: unit -> string) (sideEffect: InvoiceRepository.SideEffect<'a>) (ct: CancellationToken) : Task<'a> =
        task {
            match sideEffect with
            | InvoiceRepository.GetById (invoiceId, continuation) ->
                let! views = Sql.query (connectionString ()) $"{Sql.selectViews} WHERE InvoiceId = @InvoiceId" [ "@InvoiceId", box invoiceId ] Sql.readView ct
                return
                    views
                    |> List.tryHead
                    |> Option.map (fun v ->
                        { Id = v.InvoiceId
                          ClientId = v.ClientId
                          ContractId = Option.ofNullable v.ContractId
                          Amount = v.Amount
                          PaymentId = Option.ofNullable v.PaymentId })
                    |> continuation
            | InvoiceRepository.Save (invoice, eventCount, continuation) ->
                let sql =
                    """
                    MERGE Invoices AS target
                    USING (SELECT @InvoiceId AS InvoiceId) AS source ON target.InvoiceId = source.InvoiceId
                    WHEN MATCHED THEN
                        UPDATE SET ClientId = @ClientId, ContractId = @ContractId, Amount = @Amount, PaymentId = @PaymentId,
                                   Version = target.Version + @EventCount
                    WHEN NOT MATCHED THEN
                        INSERT (InvoiceId, ClientId, ContractId, Amount, PaymentId, Version)
                        VALUES (@InvoiceId, @ClientId, @ContractId, @Amount, @PaymentId, @EventCount);
                    """

                do!
                    Sql.execute
                        (connectionString ())
                        sql
                        [ "@InvoiceId", box invoice.Id
                          "@ClientId", box invoice.ClientId
                          "@ContractId", Sql.dbValue (Option.toNullable invoice.ContractId)
                          "@Amount", box invoice.Amount
                          "@PaymentId", Sql.dbValue (Option.toNullable invoice.PaymentId)
                          "@EventCount", box eventCount ]
                        ct

                return continuation ()
        }

module InvoiceReadModelImpl =
    let handle<'a> (connectionString: unit -> string) (sideEffect: InvoiceReadModel.SideEffect<'a>) (ct: CancellationToken) : Task<'a> =
        task {
            match sideEffect with
            | InvoiceReadModel.GetAll continuation ->
                let! views = Sql.query (connectionString ()) Sql.selectViews [] Sql.readView ct
                return continuation views
            | InvoiceReadModel.GetById (invoiceId, continuation) ->
                let! views = Sql.query (connectionString ()) $"{Sql.selectViews} WHERE InvoiceId = @InvoiceId" [ "@InvoiceId", box invoiceId ] Sql.readView ct
                return views |> List.tryHead |> continuation
        }
