using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Domain
{
    public abstract class Identity<TValue> : SingleValueObject<TValue>
    {
        protected Identity(TValue value)
            :base(value)
        {
        }
    }
}
