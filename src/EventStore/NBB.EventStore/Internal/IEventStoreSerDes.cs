// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.EventStore.Internal
{
    public interface IEventStoreSerDes
    {
        string Serialize(object obj);
        object Deserialize(string strObj, Type type);
    }
}
