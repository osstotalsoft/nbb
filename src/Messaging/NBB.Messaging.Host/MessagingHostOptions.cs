// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.Host
{
    public class MessagingHostOptions
    {
        public TransportErrorStrategy TransportErrorStrategy { get; set; } = TransportErrorStrategy.Retry;
        public int StartRetryCount { get; set; } = 10;
        public int RestartDelaySeconds { get; set;} = 10;
    }

    public enum TransportErrorStrategy
    {
        Throw = 0,
        Retry = 1
    }
}
