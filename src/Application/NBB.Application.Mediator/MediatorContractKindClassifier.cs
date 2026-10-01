// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Mediator;
using NBB.Core.Abstractions;

namespace NBB.Application.Mediator
{
    /// <summary>
    /// Classifies Mediator contracts: requests and commands without a response (<see cref="IRequest"/>, <see cref="ICommand"/>) are commands,
    /// queries and requests or commands with a response are queries, <see cref="INotification"/> is an event, anything else is another type.
    /// </summary>
    public sealed class MediatorContractKindClassifier : IContractKindClassifier
    {
        public ContractKind Classify(Type contractType)
        {
            if (typeof(IRequest<Unit>).IsAssignableFrom(contractType) || typeof(ICommand<Unit>).IsAssignableFrom(contractType))
                return ContractKind.Command;

            if (typeof(IBaseQuery).IsAssignableFrom(contractType) || typeof(IBaseRequest).IsAssignableFrom(contractType) || typeof(IBaseCommand).IsAssignableFrom(contractType))
                return ContractKind.Query;

            if (typeof(INotification).IsAssignableFrom(contractType))
                return ContractKind.Event;

            return ContractKind.Other;
        }
    }
}
