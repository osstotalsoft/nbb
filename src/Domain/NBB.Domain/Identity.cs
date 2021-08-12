// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Domain
{
    public abstract record Identity<TValue>(TValue Value) : SingleValueObject<TValue>(Value);
}
