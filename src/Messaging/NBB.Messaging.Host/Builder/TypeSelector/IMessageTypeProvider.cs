using System;
using System.Collections.Generic;

namespace NBB.Messaging.Host.Builder.TypeSelector
{
    public interface IMessageTypeProvider
    {
        IEnumerable<Type> GetTypes();
        void RegisterTypes(IEnumerable<Type> types);
        void RegisterTypes(IMessageTypeProvider provider);
    }
}
