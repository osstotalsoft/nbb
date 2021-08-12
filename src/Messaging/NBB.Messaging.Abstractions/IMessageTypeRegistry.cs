﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageTypeRegistry
    {
        Type ResolveType(string messageTypeId, IEnumerable<Assembly> scannedAssemblies);

        string GetTypeId(Type messageType);
    }
}