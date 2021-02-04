using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Host.Builder.TypeSelector;
using System;

namespace NBB.Messaging.Host.Builder
{
    public class MessagingHostBuilder
    {
        public IServiceCollection ServiceCollection { get; }

        public MessagingHostBuilder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }

        /// <summary>
        /// Adds the subscriber services to the messaging host.
        /// The subscriber services are background services (hosted services) that consume messages from the bus.
        /// </summary>
        /// <param name="builder">The builder is used to configure the message types or topics for which subscriber services are added.</param>
        /// <returns>The options builder</returns>
        public MessagingHostOptionsBuilder AddSubscriberServices(Action<ITypeSourceSelector> builder)
        {
            var subscriberServiceSelector = new TypeSourceSelector(ServiceCollection);
            builder?.Invoke(subscriberServiceSelector);

            var optionsBuilder =
                new MessagingHostOptionsBuilder(ServiceCollection, subscriberServiceSelector, subscriberServiceSelector);

            return optionsBuilder;
        }
    }
}
