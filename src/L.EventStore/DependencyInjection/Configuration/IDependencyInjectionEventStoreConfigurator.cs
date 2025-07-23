using L.EventStore.Abstractions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection.Configuration;

public interface IDependencyInjectionEventStoreConfigurator
{
    EventStoreConfiguration Configuration { get; }
    IServiceCollection Services { get; }
}
