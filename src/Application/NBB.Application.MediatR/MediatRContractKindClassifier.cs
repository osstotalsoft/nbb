// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using MediatR;
using NBB.Core.Abstractions;

namespace NBB.Application.MediatR
{
    /// <summary>
    /// Classifies MediatR contracts: <see cref="IRequest"/> is a command, <see cref="IRequest{TResponse}"/> is a query,
    /// <see cref="INotification"/> is an event, anything else is another type.
    /// </summary>
    public sealed class MediatRContractKindClassifier : IContractKindClassifier
    {
        public ContractKind Classify(Type contractType)
        {
            if (typeof(IRequest).IsAssignableFrom(contractType))
                return ContractKind.Command;

            if (ImplementsGeneric(contractType, typeof(IRequest<>)))
                return ContractKind.Query;

            if (typeof(INotification).IsAssignableFrom(contractType))
                return ContractKind.Event;

            return ContractKind.Other;
        }

        private static bool ImplementsGeneric(Type type, Type genericInterface)
            => Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterface);
    }
}
