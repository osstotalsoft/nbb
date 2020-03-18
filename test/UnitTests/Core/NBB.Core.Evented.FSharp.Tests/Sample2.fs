namespace NBB.Core.Evented.FSharp.Tests

open NBB.Core.Evented.FSharp

module Sample2 =
    type Payment = {
        Id: int
        Amount: decimal
        Cancelled: bool
    }

    type DomainEvent = 
        | Added of Id:int * Amount:decimal
        | Cancelled of Id:int

    let create (id,amount) = 
        evented{
            let payment = {Id=id; Amount=amount; Cancelled=false}
            do! addEvent <| Added(id,amount)
            return payment
        }

    let cancell payment = 
        evented{
            let payment' = {payment with Cancelled=true}
            do! addEvent <| Cancelled payment.Id
            return payment'
        }

    let createCancelled = create >=> cancell

