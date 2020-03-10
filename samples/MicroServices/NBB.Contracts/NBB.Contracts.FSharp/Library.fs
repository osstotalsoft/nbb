namespace NBB.Contracts.FSharp

open NBB.Core.Effects.FSharp

module Domain =
    module ContractAggregate =
        type ContractStatus = Draft | Validated

        type Contract = {
            ContractId: int
            Value: decimal
            Status: ContractStatus
        }

        type DomainEvent = 
            | ContractCreated of Contract
            | ContractValidated of Contract

        let create contractId value = { ContractId = contractId; Value = value; Status = Draft }
        let validate contract = { contract with Status = Validated }


module Data =
    module Contract =
        open Domain.ContractAggregate
        let loadById contractId = Effect.pure' { ContractId = contractId; Value = 78m; Status = Draft; }
        let save (contract:Contract) = Effect.pure' contract |> Effect.ignore


module Application =
    open Domain.ContractAggregate
    open Data.Contract
    open NBB.Messaging.Effects

    type Event =
        | ContractCreated of Contract
        | ContractValidated of Contract

    type Command =
        | CreteContract of int * decimal
        | ValidateContract of int


    let handle cmd = 
        match cmd with
        | CreteContract (contractId, value) ->
            effect {
                let contract = create contractId value
                do! save contract
                do! MessageBus.Publish (ContractCreated contract) |> Effect.ignore
            }

        | ValidateContract contractId ->
            effect {
                let! contract = loadById contractId
                let contract' = contract |> validate
                do! contract' |> save
                do! MessageBus.Publish (ContractValidated contract) |> Effect.ignore
            }
