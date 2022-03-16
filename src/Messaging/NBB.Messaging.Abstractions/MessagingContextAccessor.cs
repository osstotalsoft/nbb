// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;

namespace NBB.Messaging.Abstractions
{
    public class MessagingContextAccessor
    {
        private static readonly AsyncLocal<MessagingContext> AsyncLocal = new();

        public MessagingContext MessagingContext
        {
            get => AsyncLocal.Value;
            set => AsyncLocal.Value = value;
        }
    }
}
