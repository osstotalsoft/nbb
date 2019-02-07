using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Core.Abstractions
{
    public interface IKeyProvider
    {
        string Key { get; }
    }
}
