using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.BackwardCompatibility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection UseTopicResolutionBackwardCompatibility(this IServiceCollection services, IConfiguration configuration)
        {
            var topicResolutionCompatibility = configuration.GetSection("Messaging")?["TopicResolutionCompatibility"];
            if (string.IsNullOrWhiteSpace(topicResolutionCompatibility) || topicResolutionCompatibility == "NBB_4")
            {
                services.Decorate<ITopicRegistry, NBB4TopicRegistryDecorator>();
            }
            return services;
        }
    }
}
