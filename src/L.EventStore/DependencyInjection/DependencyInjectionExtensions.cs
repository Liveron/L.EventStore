using L.EventStore.Configuration;
using L.EventStore.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void AddEventStore(this IServiceCollection services, 
        Action<IEventBusConfigurator>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var configurator = new DependencyInjectionEventBusConfigurator(services);
        configure?.Invoke(configurator);
        configurator.Complete();
    }
}
