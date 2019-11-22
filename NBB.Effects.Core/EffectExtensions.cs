using NBB.Effects.Core.Program;

namespace NBB.Effects.Core
{
    public static class EffectExtensions
    {
        public static IProgram<T> Lift<T>(this IEffect<T> effect)
        {
            return FreeProgram<T>.From(effect);
        }
    }
}
