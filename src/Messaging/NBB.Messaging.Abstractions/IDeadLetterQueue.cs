// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Messaging.Abstractions
{
    public interface IDeadLetterQueue
    {
        void Push(TransportReceivedData messageData, string topicName, Exception ex);
        
        void Push(MessagingEnvelope messageEnvelope, string topicName, Exception ex);
    }
}
