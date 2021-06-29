using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Effects;
using NBB.Messaging.InProcessMessaging.Extensions;

namespace NBB.ProjectR.Tests
{
    public class TestFixture
    {
        public ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddProjectR(GetType().Assembly);
            services.AddMediatR(GetType().Assembly);
            services
                .AddEffects()
                .AddMessagingEffects()
                .AddMediatorEffects();
            services.AddMessageBus().AddInProcessTransport();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            return services.BuildServiceProvider();

        }
    }
}
