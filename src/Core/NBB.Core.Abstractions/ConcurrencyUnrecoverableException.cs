// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Core.Abstractions
{
    public class ConcurrencyUnrecoverableException : Exception
    {
        public ConcurrencyUnrecoverableException(string msg, Exception inner)
            : base(msg, inner)
        {
        }

        public ConcurrencyUnrecoverableException(string msg)
            : base(msg)
        {
        }
    }
}
