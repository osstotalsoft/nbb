namespace NBB.Core.Effects
{

    public interface ISideEffect<out TOutput>
    {
    }

    public interface ISideEffect : ISideEffect<Unit>
    {
    }
}
