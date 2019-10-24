using System;

namespace NBB.ProcessManager.Definition.Effects
{
    public class CombinedEffect : Effect
    {
        public IEffect Effect1 { get; }
        public IEffect Effect2 { get; }

        public CombinedEffect(IEffect effect1, IEffect effect2)
        {
            Effect1 = effect1;
            Effect2 = effect2;
        }
    }
}