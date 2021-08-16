// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    public interface IMessagingHostBuilder
    {
        IMessagingHostBuilder Configure(Action<IMessagingHostConfigurationBuilder> configure);
        IMessagingHostBuilder Configure(Func<IMessagingHostConfigurationBuilder, Task> configure);
        IMessagingHostBuilder UseStartup<TStartup>() where TStartup : class, IMessagingHostStartup;
    }
    public class MessagingHostBuilder : IMessagingHostBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public MessagingHostBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IMessagingHostBuilder Configure(Action<IMessagingHostConfigurationBuilder> configure)
        {
            _serviceCollection.AddSingleton<IMessagingHostStartup>(new DelegateMessagingHostStartup(configure));
            return this;
        }

        public IMessagingHostBuilder Configure(Func<IMessagingHostConfigurationBuilder, Task> configure)
        {
            _serviceCollection.AddSingleton<IMessagingHostStartup>(new AsyncDelegateMessagingHostStartup(configure));
            return this;
        }

        public IMessagingHostBuilder UseStartup<TStartup>() where TStartup : class, IMessagingHostStartup
        {
            _serviceCollection.AddSingleton<IMessagingHostStartup, TStartup>();
            return this;
        }
    }

}
