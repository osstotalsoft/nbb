using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IMessagingHostStartup
    {
        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder);
    }

    public class DelegateMessagingHostStartup : IMessagingHostStartup
    {
        private readonly Action<IMessagingHostConfigurationBuilder> _configure;

        public DelegateMessagingHostStartup(Action<IMessagingHostConfigurationBuilder> configure) 
            => _configure = configure;

        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
        {
            _configure.Invoke(hostConfigurationBuilder);
            return Task.CompletedTask;
        }
    }

    public class AsyncDelegateMessagingHostStartup : IMessagingHostStartup
    {
        private readonly Func<IMessagingHostConfigurationBuilder, Task> _configure;

        public AsyncDelegateMessagingHostStartup(Func<IMessagingHostConfigurationBuilder, Task> configure)
            => _configure = configure;

        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
            => _configure.Invoke(hostConfigurationBuilder);
    }
}