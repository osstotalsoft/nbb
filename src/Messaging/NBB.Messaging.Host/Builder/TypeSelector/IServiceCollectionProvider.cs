using Microsoft.Extensions.DependencyInjection;

namespace NBB.Messaging.Host.Builder.TypeSelector
{
    public interface IServiceCollectionProvider
    {
        IServiceCollection ServiceCollection { get; }
    }
}
