// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Core.Abstractions
{
    /// <summary>
    /// Classifies application contracts as commands, queries, events or other types.
    /// An application has a single implementation, registered by its mediator application adapter
    /// (<c>AddMediatorIntegration()</c> in NBB.Application.Mediator or <c>AddMediatRIntegration()</c> in NBB.Application.MediatR).
    /// </summary>
    public interface IContractKindClassifier
    {
        ContractKind Classify(Type contractType);
    }
}
