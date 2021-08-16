// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Core.Abstractions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        public ConcurrencyException(string msg)
            : base(msg)
        {
        }
    }
}
