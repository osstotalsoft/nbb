// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.EventStore.AdoNet.Multitenancy.Internal
{
    public class Scripts : AdoNet.Internal.Scripts
    {
        public Scripts()
            : base(typeof(Scripts).Assembly, "NBB.EventStore.AdoNet.MultiTenancy.Internal.SqlScripts")
        {
        }
    }
}
