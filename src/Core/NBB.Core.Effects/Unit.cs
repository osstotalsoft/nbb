using System;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Value = new Unit();
        public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

        public override int GetHashCode() => 0;
        public bool Equals(Unit other) => true;
        public override bool Equals(object obj) => obj is Unit;
        public static bool operator ==(Unit _first, Unit _second) => true;
        public static bool operator !=(Unit _first, Unit _second) => false;
    }
}
