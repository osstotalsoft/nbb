using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
    public class NoEffect
    {
        public static IEffect<Unit> Instance = new Effect<Unit>(runner => Unit.Task);
    }
}