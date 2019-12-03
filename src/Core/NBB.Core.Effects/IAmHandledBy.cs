namespace NBB.Core.Effects
{
    public interface IAmHandledBy<in THandler> 
        where THandler: ISideEffectHandler
    {
    }
}
