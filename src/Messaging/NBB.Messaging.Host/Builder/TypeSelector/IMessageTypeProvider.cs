// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IMessageTypeProvider
    {
        IEnumerable<Type> GetTypes();
        void RegisterTypes(IEnumerable<Type> types);
        void RegisterTypes(IMessageTypeProvider provider);
    }
}
