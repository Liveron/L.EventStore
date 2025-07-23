using L.EventStore.Projector.Configuration;
using L.EventStore.Projector.Configuration.EventHandling;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.Projector.DependencyInjection.Configuration;

public interface IDependencyInjectionEventStoreProjectorConfigurator
    : IProjectorSubscriptionsConfigurator
{
    public IServiceCollection Services { get; }
}
