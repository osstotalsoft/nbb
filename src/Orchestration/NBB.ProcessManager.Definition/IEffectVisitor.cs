using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using NBB.ProcessManager.Definition.Effects;

namespace NBB.ProcessManager.Definition
{
    public interface IEffectVisitor
    {
        Task<TResult> Visit<TResult>(HttpEffect<TResult> effect);
        Task<Unit> Visit(CancelTimeoutsEffect effect);
        Task<Unit> Visit(ParallelEffect effect);
        Task<Unit> Visit(NoEffect effect);
        Task<Unit> Visit(PublishMessageEffect effect);
        Task<TResult> Visit<TResult>(SendQueryEffect<TResult> effect);
        Task<Unit> Visit(RequestTimeoutEffect effect);
        Task<Unit> Visit(SequentialEffect effect);
        Task<TEffectResult2> Visit<TEffectResult1, TEffectResult2>(BoundedEffect<TEffectResult1, TEffectResult2> effect);
    }
}