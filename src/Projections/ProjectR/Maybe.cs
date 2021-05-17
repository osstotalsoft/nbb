using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectR
{
    public record Maybe<T>
    {
        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public Maybe()
        {
            Value = default;
            HasValue = false;
        }

        public T Value { get; }

        public bool HasValue { get;  }

        public static implicit operator Maybe<T>(T value) => new(value);

        public static Maybe<T> Nothing { get; } = new();

        public static void Asa()
        {
            Maybe<int> x = 1;
            Maybe<int> y = Maybe<int>.Nothing;
        }
    }
}
