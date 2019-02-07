using System;

namespace NBB.GetEventStore.Internal
{
    public interface ISerDes
    {
        T Deserialize<T>(byte[] data);
        object Deserialize(byte[] data, Type type);

        byte[] Serialize(object obj);
    }
}
