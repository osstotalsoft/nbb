using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Core.Abstractions
{
    public interface IMementoProvider
    {
        object CreateMemento();
        void SetMemento(object memento);
    }

    public interface IMementoProvider<TMemento> : IMementoProvider
    {
        new TMemento CreateMemento();
        void SetMemento(TMemento memento);
    }
}
