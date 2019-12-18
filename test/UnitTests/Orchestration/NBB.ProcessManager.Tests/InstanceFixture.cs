using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Effects;
using NBB.EventStore.InMemory;
using NBB.EventStore.Internal;
using NBB.ProcessManager.Runtime.Persistence;

namespace NBB.ProcessManager.Tests
{
    public class InstanceDataRepositoryFixture
    {
        public InstanceDataRepository Repository { get; private set; }

        public InstanceDataRepositoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<EventStore.EventStore>>();
            var interpreter = new Interpreter(new SideEffectHandlerFactoryMock());

            Repository = new InstanceDataRepository(
                new EventStore.EventStore(new InMemoryRepository(), new NewtonsoftJsonEventStoreSerDes(), logger),
                interpreter);
        }
    }

    public class SideEffectHandlerFactoryMock : ISideEffectHandlerFactory
    {
        public ISideEffectHandler<ISideEffect<TOutput>, TOutput> GetSideEffectHandlerFor<TOutput>(ISideEffect<TOutput> sideEffect)
        {
            var mock = new Mock<ISideEffectHandler<ISideEffect<TOutput>, TOutput>>();
            mock.Setup(x => x.Handle(It.IsAny<ISideEffect<TOutput>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(TOutput)));

            return mock.Object;
        }
    }
}