using L.EventStore.Abstractions;
using L.EventStore.Abstractions.Configuration;
using L.EventStore.Abstractions.Configuration.StreamId;
using L.EventStore.Configuration;
using L.EventStore.DependencyInjection.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IDependencyInjectionEventStoreConfigurator AddEventStore(
        this IServiceCollection services, Action<IEventStoreConfigurator>? configure = null)
    {
        var configurator = new EventStoreConfigurator();
        configure?.Invoke(configurator);

        var configuration = configurator.Configuration;

        services.AddEventTypeRepository(configuration);
        services.AddEventStore(configuration);

        return new DependencyInjectionEventStoreConfigurator(services, configuration);
    }

    private static void AddEventStore(this IServiceCollection services, EventStoreConfiguration configuration)
    {
        var eventStreamIdType = GetStreamIdType(configuration.StreamIdType);
        var eventType = configuration.EventType;

        if (eventType == typeof(object))
        {
            services.RegisterEventStore(eventStreamIdType);
        }
        else
        {
            services.RegisterEventStore(eventStreamIdType, eventType);
        }
    }

    private static void RegisterEventStore(this IServiceCollection services, Type streamIdType)
    {
        var eventStoreInterfaceType = typeof(IEventStore<>).MakeGenericType(streamIdType);
        var eventStoreType = typeof(EventStore<>).MakeGenericType(streamIdType);
        services.AddScoped(eventStoreInterfaceType, eventStoreType);
    }

    private static void RegisterEventStore(this IServiceCollection services, Type streamIdType, Type eventType)
    {
        var eventStoreInterfaceType1 = typeof(IEventStore<>).MakeGenericType(streamIdType);
        var eventStoreInterfaceType2 = typeof(IEventStore<,>).MakeGenericType(streamIdType, eventType);
        var eventStoreType = typeof(EventStore<,>).MakeGenericType(streamIdType, eventType);
        services.AddScoped(eventStoreInterfaceType1, eventStoreType);
        services.AddScoped(eventStoreInterfaceType2, eventStoreType);
    }

    private static Type GetStreamIdType(StreamIdType streamIdType)
    {
        return streamIdType switch
        {
            StreamIdTypes.Guid => typeof(Guid),
            _ => throw new InvalidOperationException($"Unsupported stream id type: {streamIdType}")
        };
    }

    private static void AddEventTypeRepository(this IServiceCollection services, EventStoreConfiguration configuration)
    {
        var eventTypeRepository = CreateEventTypeRepository(configuration.EventType);
        services.AddSingleton<IEventTypeRepository>(eventTypeRepository);
    }

    private static EventTypeRepository CreateEventTypeRepository(Type eventType)
    {
        var events = FindEvents(eventType);
        var eventTypeRepository = new EventTypeRepository();

        foreach (var type in events)
            eventTypeRepository.AddEventTypeIfNotExist(type.Name, type);

        return eventTypeRepository;
    }

    private static Type[] FindEvents(Type eventType)
    {
        return eventType.Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && eventType.IsAssignableFrom(t))
            .ToArray() ?? [];
    }
}
