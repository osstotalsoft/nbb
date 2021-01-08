using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public struct Unit
    {
        public static readonly Unit Value = new Unit();
        public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);
    }
}
