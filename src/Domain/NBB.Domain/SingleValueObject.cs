// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Domain
{
    public abstract record SingleValueObject<TValue>(TValue Value) {
        public override string ToString() => Value.ToString();
    }
}
