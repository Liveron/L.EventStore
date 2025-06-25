using L.EventStore.Abstractions;
using L.EventStore.Configuration;
using L.EventStore.Configuration.StreamId;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection.Configuration;

public class DependencyInjectionEventBusConfigurator(
    IServiceCollection services) : IDependencyInjectionEventBusConfigurator
{
    public IServiceCollection Services { get; init; } = services;
    public EventBusConfiguration Configuration { get; init; } =
        new DependencyInjectionEventBusConfiguration();

    public void SetEventStreamIdType<TEventStreamId>() where TEventStreamId : StreamIdType
    {
        Configuration.StreamIdType = typeof(TEventStreamId);
    }

    public void SetEventType<TEvent>() where TEvent : notnull
    {
        Configuration.EventType = typeof(TEvent);
    }

    public void Complete()
    {
        var eventStoreType = typeof(EventStore<,>).MakeGenericType(Configuration.StreamIdType, Configuration.EventType);
        var eventStoreInterfaceType = typeof(IEventStore<,>).MakeGenericType(Configuration.StreamIdType, Configuration.EventType);
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
