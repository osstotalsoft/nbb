using NBB.Messaging.DataContracts;
using System;
using System.Collections.Generic;

namespace NBB.Messaging.DataContracts
{
    public interface IMessagingResponse //: IMessage
    {
        Guid RequestId { get; }
    }

    public class MessagingResponse<TResponse> : IMessagingResponse
        where TResponse : class
    {
        public TResponse Response { get; }
        public Guid RequestId { get; }

        public MessagingResponse(TResponse response, Guid requestId)
        {
            Response = response;
            RequestId = requestId;
        }
    }
}