using L.EventStore.Abstractions;
using L.EventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection.Configuration;

public class DependencyInjectionEventBusConfigurator(
    IServiceCollection services) : IDependencyInjectionEventBusConfigurator
{
    public IServiceCollection Services { get; init; } = services;
    public EventBusConfiguration Configuration { get; init; } =
        new DependencyInjectionEventBusConfiguration();

    public void SetEventStreamIdType<TEventStreamId>() where TEventStreamId 
        : IEquatable<TEventStreamId>, IComparable<TEventStreamId>
    {
        Configuration.StreamIdType = typeof(TEventStreamId) 
            ?? throw new ArgumentNullException(nameof(TEventStreamId), 
                "StreamIdType cannot be null. Please specify a valid type for the event stream identifier.");
    }

    public void Complete()
    {
        var eventStoreType = typeof(EventStore<>).MakeGenericType(Configuration.StreamIdType);
        var eventStoreInterfaceType = typeof(IEventStore<>).MakeGenericType(Configuration.StreamIdType);
        var repositoryInterfaceType = typeof(IEventStoreRepository<>).MakeGenericType(Configuration.StreamIdType);

        var hasRepository = Services.Any(s => s.ServiceType == repositoryInterfaceType);
        if (!hasRepository)
        {
            throw new InvalidOperationException(
                $"No repository registered for type '{repositoryInterfaceType.FullName}'. " +
                "Please register an implementation of IEventStoreRepository<TStreamIdentifier>.");
        }

        Services.AddScoped(eventStoreInterfaceType, eventStoreType);
    }
}
