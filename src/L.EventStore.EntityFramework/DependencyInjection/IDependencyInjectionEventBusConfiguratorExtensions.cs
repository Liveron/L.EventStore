using L.EventStore.Abstractions;
using L.EventStore.Configuration.StreamId;
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

        switch (streamIdType)
        {
            case Type t when t == typeof(StreamIdTypes.Guid):
                configurator.Services.AddScoped(typeof(IEventStoreRepository<Guid>), typeof(EventStoreRepository<Guid, TContext>));
                break;
            default:
                throw new NotSupportedException($"StreamId type '{streamIdType.FullName}' is not supported by Entity Framework Event Store.");
        }
    }
}
