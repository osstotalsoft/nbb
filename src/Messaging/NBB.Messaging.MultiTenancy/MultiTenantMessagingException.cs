using System;

namespace NBB.Messaging.MultiTenancy
{
    [Serializable]
    public class MultiTenantMessagingException : Exception
    {
        public MultiTenantMessagingException(string message) : base(message)
        {
        }
    }
}