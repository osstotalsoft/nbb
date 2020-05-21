using System.Collections.Generic;

namespace NBB.Domain
{
    public abstract class SingleValueObject<TValue> : ValueObject
    {
        public TValue Value { get; }

        protected SingleValueObject(TValue value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return ReferenceEquals(Value, null)
                ? string.Empty
                : Value.ToString();
        }
    }
}
