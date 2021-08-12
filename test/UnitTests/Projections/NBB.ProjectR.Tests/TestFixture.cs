// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddEventStore()
                .WithNewtownsoftJsonEventStoreSeserializer()
                .WithInMemoryEventRepository();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            return services.BuildServiceProvider();

        }
    }
}
