using MediatR;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.Effects;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.EffectRunners
{

    public class NoOpVisitor : IEffectVisitor
    {
        public Task<TResult> Visit<TResult>(HttpEffect<TResult> effect)
        {
            return Task.FromResult(default(TResult));
        }

        public Task<Unit> Visit(CancelTimeoutsEffect effect)
        {
            return Unit.Task;
        }

        public Task<TResult[]> Visit<TResult>(ParallelEffect<TResult> effect)
        {
            return Task.FromResult(default(TResult[]));
        }

        public Task<Unit> Visit(NoEffect effect)
        {
            return Unit.Task;
        }

        public Task<Unit> Visit(PublishMessageEffect effect)
        {
            return Unit.Task;
        }

        public Task<TResult> Visit<TResult>(SendQueryEffect<TResult> effect)
        {
            return Task.FromResult(default(TResult));
        }

        public Task<Unit> Visit(RequestTimeoutEffect effect)
        {
            return Unit.Task;
        }

        public Task<Unit> Visit(SequentialEffect effect)
        {
            return Unit.Task;
        }

        public Task<TEffectResult2> Visit<TEffectResult1, TEffectResult2>(BoundedEffect<TEffectResult1, TEffectResult2> effect)
        {
            return Task.FromResult(default(TEffectResult2));
        }
    }
}