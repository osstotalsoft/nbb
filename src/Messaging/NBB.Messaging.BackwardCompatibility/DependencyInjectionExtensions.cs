using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.BackwardCompatibility
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
