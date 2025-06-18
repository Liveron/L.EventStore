using L.EventStore.Abstractions;
using L.EventStore.DependencyInjection.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.EntityFramework.DependencyInjection;

public static class IDependencyInjectionEventBusConfiguratorExtensions
{
    public static void AddEntityFramework<TContext>(this IDependencyInjectionEventBusConfigurator configurator)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configurator, nameof(configurator));

        var streamIdType = configurator.Configuration.StreamIdType;
        var repositoryInterfaceType = typeof(IEventStoreRepository<>).MakeGenericType(streamIdType);
        var repositoryType = typeof(EventStoreRepository<,>).MakeGenericType(streamIdType, typeof(TContext));

        configurator.Services.AddScoped(repositoryInterfaceType, repositoryType);
    }
}
