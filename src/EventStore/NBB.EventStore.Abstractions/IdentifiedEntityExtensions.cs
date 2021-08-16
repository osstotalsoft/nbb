// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;

namespace NBB.EventStore.Abstractions
{
    public static class IdentifiedEntityExtensions
    {
        public static string GetStream(this IIdentifiedEntity entity)
        {
            return entity.GetStreamFor(entity.GetIdentityValue());
        }

        public static string GetStreamFor(this IIdentifiedEntity entity, object identity)
        {
            return entity.GetTypeId() + ":" + identity;
        }
    }
}
