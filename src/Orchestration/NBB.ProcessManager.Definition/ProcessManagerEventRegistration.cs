// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.ProcessManager.Definition
{
    /// <summary>
    /// An event handled by a process manager definition. Mediator adapters register one in-process handler per registration.
    /// </summary>
    /// <param name="DefinitionType">The process manager definition type.</param>
    /// <param name="DataType">The process manager instance data type.</param>
    /// <param name="EventType">The handled event type.</param>
    public record ProcessManagerEventRegistration(Type DefinitionType, Type DataType, Type EventType);
}
