using System.Threading.Tasks;
using NBB.Effects.Core.Program;

namespace NBB.Effects.Core.Interpreter
{
    public interface IInterpreter
    {
        Task<T> Interpret<T>(IProgram<T> program);
    }
}
