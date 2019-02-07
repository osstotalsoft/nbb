using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Core.Abstractions
{
    public sealed class SetOnce<T>
    {
        private T _value;
        private string _name;

        public SetOnce(string name = null)
        {
            _name = name;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this._value, default(T)))
                    throw new InvalidOperationException($"{_name ?? nameof(Value)} is already set");

                this._value = value;
            }
        }

        public static implicit operator T(SetOnce<T> value) { return value.Value; }
    }
 
}
