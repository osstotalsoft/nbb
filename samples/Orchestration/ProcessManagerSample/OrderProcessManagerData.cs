// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace ProcessManagerSample
{
    public record OrderProcessManagerData
    {
        public Guid OrderId { get; init; }
        public bool IsPaid { get; init; }
    }
}