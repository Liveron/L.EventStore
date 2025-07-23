using L.EventStore.Abstractions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection.Configuration;
public class DependencyInjectionEventStoreConfigurator(
    IServiceCollection services, EventStoreConfiguration configuration)
    : IDependencyInjectionEventStoreConfigurator
{
    public EventStoreConfiguration Configuration => configuration;
    public IServiceCollection Services => services;
}
