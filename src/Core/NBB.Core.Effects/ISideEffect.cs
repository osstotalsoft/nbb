namespace NBB.Core.Effects
{

    public interface ISideEffect<out TResult>
    {
    }

    public interface ISideEffect : ISideEffect<Unit>
    {
    }
}
