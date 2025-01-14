// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace NBB.MultiTenancy.Abstractions
{
    public sealed class TenantLogScope(TenantContext tenantContext) : IReadOnlyList<KeyValuePair<string, object>>
    {
        private string? _cachedToString;

        public Guid TenantId => tenantContext?.GetTenantId() ?? throw new ArgumentNullException(nameof(tenantContext));
        public string TenantCode => tenantContext?.GetTenantCode() ?? throw new ArgumentNullException(nameof(tenantContext));
        public int Count => 2;

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>(nameof(TenantId), TenantId);
                }
                else if (index == 1)
                {
                    return new KeyValuePair<string, object>(nameof(TenantCode), TenantCode);
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public override string ToString()
        {
            _cachedToString ??= string.Format(
                    CultureInfo.InvariantCulture,
                    "TenantId:{0} TenantCode:{1}",
                    TenantId,
                    TenantCode);

            return _cachedToString;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
