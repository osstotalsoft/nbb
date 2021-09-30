// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IDeadLetterQueue
    {
        void Push(byte[] messageData, string topicName, Exception ex);
        void Push(byte[] payloadData, IDictionary<string, string> headers,  string topicName, Exception ex);
        void Push(MessagingEnvelope messageEnvelope, string topicName, Exception ex);
    }
}
