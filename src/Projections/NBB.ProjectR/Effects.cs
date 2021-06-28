using NBB.Core.Effects;

namespace NBB.ProjectR
{
    public static class Cmd
    {
        public static readonly Effect<Unit> None = Effect.Pure(Unit.Value);
        
    }
}
