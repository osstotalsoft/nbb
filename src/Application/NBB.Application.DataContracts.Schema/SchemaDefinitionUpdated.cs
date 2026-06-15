// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using Mediator;

namespace NBB.Application.DataContracts.Schema
{
    public record SchemaDefinitionUpdated(
        List<SchemaDefinition> Definitions,
        string ApplicationName
    ) : INotification;
}
