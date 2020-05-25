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
