using L.EventStore.Abstractions.Configuration.StreamId;
using L.EventStore.Projector.Configuration;
using L.EventStore.Projector.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace L.EventStore.Projector.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IDependencyInjectionEventStoreProjectorConfigurator AddEventStoreProjector(
        this IServiceCollection services, Action<IEventStoreProjectorConfigurator>? configure = null)
    {
        var configurator = new EventStoreProjectorConfigurator();
        configure?.Invoke(configurator);

        services.AddEventStoreProjector(configurator.Configuration);

        return new DependencyInjectionEventStoreProjectorConfigurator(services);
    }

    private static void AddEventStoreProjector(this IServiceCollection services, EventStoreProjectorConfiguration configuration)
    {
        var eventStoreProjectorType = configuration.StreamIdType switch
        {
            StreamIdTypes.Guid => typeof(ProjectorBackgroundService<Guid>),
            _ => throw new NotSupportedException(
                                $"StreamIdType '{configuration.StreamIdType.GetType().Name}' is not supported."),
        };

        services.AddHostedService(sp => ActivatorUtilities.CreateInstance<BackgroundService>(sp, eventStoreProjectorType));
    }
}
