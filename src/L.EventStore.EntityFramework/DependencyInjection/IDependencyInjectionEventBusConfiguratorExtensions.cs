using L.EventStore.Abstractions;
using L.EventStore.Abstractions.Configuration.StreamId;
using L.EventStore.DependencyInjection.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.EntityFramework.DependencyInjection;

public static class IDependencyInjectionEventBusConfiguratorExtensions
{
    public static void AddEntityFramework<TContext>(
        this IDependencyInjectionEventStoreConfigurator configurator)
        where TContext : DbContext
    {
        var services = configurator.Services;
        var configuration = configurator.Configuration;

        Type eventStoreRepositoryInterfaceType;
        Type eventStoreRepositoryType;

        var streamIdType = configuration.StreamIdType;
        switch (configuration.StreamIdType)
        {
            case StreamIdTypes.Guid:
                eventStoreRepositoryInterfaceType = typeof(IEventStoreRepository<Guid>);
                eventStoreRepositoryType = typeof(EventStoreRepository<Guid, TContext>);
                break;
            default:
                throw new NotSupportedException($"StreamId type '{streamIdType}' is not supported.");
        }

        services.AddScoped(eventStoreRepositoryInterfaceType, eventStoreRepositoryType);
    }
}
