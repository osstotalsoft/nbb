using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.EventStore.InMemory;
using NBB.EventStore.Internal;
using NBB.ProcessManager.Runtime.Persistence;
using System.Threading.Tasks;

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
            Repository = new InstanceDataRepository(
                new EventStore.EventStore(new InMemoryRepository(), new NewtonsoftJsonEventStoreSerDes(), logger),
                type => effect => Task.CompletedTask);
        }
    }
}