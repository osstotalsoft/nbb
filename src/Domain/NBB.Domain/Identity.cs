namespace NBB.Domain
{
    public abstract record Identity<TValue>(TValue Value) : SingleValueObject<TValue>(Value);
}
