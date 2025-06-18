using L.EventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection.Configuration;

public interface IDependencyInjectionEventBusConfigurator : IEventBusConfigurator
{
    public IServiceCollection Services { get; init; }
}
