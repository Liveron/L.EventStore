using L.EventStore.Abstractions;
using L.EventStore.Projector.Abstractions;
using L.EventStore.Projector.Configuration.EventHandling;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace L.EventStore.Projector.Services;

public sealed class ProjectorService<TStreamIdentifier>(EventSubscriptionsInfo options)
    : IProjectorService<TStreamIdentifier> where TStreamIdentifier 
    : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    private readonly EventSubscriptionsInfo _eventSubscriptionsInfo = options
        ?? throw new ArgumentNullException(nameof(options));

    public ProjectorService(IOptions<EventSubscriptionsInfo> options) : this(options.Value) { }

    public async Task ProjectAsync(List<EventEntry<TStreamIdentifier>> eventEntries)
    {
        foreach (var entry in eventEntries)
        {
            var eventType = _eventSubscriptionsInfo.EventTypes[entry.EventType] 
                ?? throw new InvalidOperationException();

            var eventHandlers = _eventSubscriptionsInfo.EventHandlers[eventType]
                ?? throw new InvalidOperationException();

            var @event = JsonSerializer.Deserialize(entry.Event, eventType)
                ?? throw new InvalidOperationException($"Cannot deserialize event {entry.Event} to type {eventType.Name}");

            await InvokeHandlersAsync(eventHandlers, @event);
        }
    }

    private static async Task InvokeHandlersAsync(List<IEventHandler> eventHandlers, object @event)
    {
        foreach (var handler in eventHandlers)
        {
            await handler.HandleAsync(@event);
        }
    }
}
