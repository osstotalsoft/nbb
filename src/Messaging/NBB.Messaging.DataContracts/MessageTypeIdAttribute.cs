﻿using System;

namespace NBB.Messaging.DataContracts
{
    public class MessageTypeIdAttribute : Attribute
    {
        public string MessageTypeId { get; }

        public MessageTypeIdAttribute(string messageTypeId)
        {
            MessageTypeId = messageTypeId;
        }
    }
}
