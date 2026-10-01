// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Core.Abstractions
{
    /// <summary>
    /// The CQRS role of an application contract (published language type), as defined by the mediator library used by the application.
    /// </summary>
    public enum ContractKind
    {
        /// <summary>A request that changes state and has no response.</summary>
        Command,

        /// <summary>A request that has a response.</summary>
        Query,

        /// <summary>A notification of something that happened.</summary>
        Event,

        /// <summary>Any other type.</summary>
        Other
    }
}
