using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime
{
    public class ProcessExecutionCoordinator
    {
        private readonly IInstanceDataRepository _dataRepository;
        private readonly IEnumerable<IDefinition> _definitions;

        public ProcessExecutionCoordinator(IInstanceDataRepository dataRepository, IEnumerable<IDefinition> definitions)
        {
            _dataRepository = dataRepository;
            _definitions = definitions;
        }

        public async Task Invoke<TDefinition, TData, TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TDefinition : IDefinition<TData>
            where TData : struct
        {
            var definition = _definitions.OfType<TDefinition>().SingleOrDefault();
            if (definition == null)
                throw new Exception($"No definition found for type {typeof(TDefinition)}");

            var identitySelector = definition.GetCorrelationFilter<TEvent>();
            if (identitySelector == null)
                throw new ArgumentNullException($"No correlation defined for eventType {typeof(TEvent)} in definition {typeof(TDefinition)}");

            var instance = await _dataRepository.Get(definition, identitySelector(@event), cancellationToken);
            instance.ProcessEvent(@event);
            await _dataRepository.Save(instance, cancellationToken);
        }
    }
}