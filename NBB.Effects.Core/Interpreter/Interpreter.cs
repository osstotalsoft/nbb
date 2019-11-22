using NBB.Effects.Core.Program;
using System.Threading.Tasks;

namespace NBB.Effects.Core.Interpreter
{
    public class Interpreter : IInterpreter
    {
        private readonly IEffectRunner _effectRunner;

        public Interpreter(IEffectRunner effectRunner)
        {
            _effectRunner = effectRunner;
        }

        public Task<T> Interpret<T>(IProgram<T> program)
        {
            return InternalInterpret(program as dynamic);
        }

        private Task<T> InternalInterpret<T>(PureProgram<T> program) => Task.FromResult(program.Value);
        private async Task<T> InternalInterpret<T>(FreeProgram<T> program)
        {
            var innerProgram = await _effectRunner.RunEffect(program.Effect);
            return await Interpret(innerProgram);
        }

        private async Task<T> InternalInterpret<T1, T2, T>(ParallelProgram<T1, T2, T> program)
        {
            var t1 = Interpret(program.LeftProgram);
            var t2 = Interpret(program.RightProgram);
            await Task.WhenAll(t1, t2);
            var nextProgram = program.Next(t1.Result, t2.Result);
            return await Interpret(nextProgram);
        }
    }
}
