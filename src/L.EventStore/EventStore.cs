using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using System.Text.Json;

namespace L.EventStore;

public sealed class EventStore<TStreamIdentifier>(
    IEventStoreRepository<TStreamIdentifier> repository)
    : IEventStore<TStreamIdentifier>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier> 
{
    private readonly IEventStoreRepository<TStreamIdentifier> _repository = repository 
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<List<IEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(nameof(streamType));

        var eventEntries = await _repository.GetEventsAsync(id, streamType);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    public async Task<List<IEvent>> GetEventStreamAsync(TStreamIdentifier id)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        var eventEntries = await _repository.GetEventsAsync(id);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    private static List<IEvent> DeserializeEvents(List<EventStoreEntry<TStreamIdentifier>> eventEntries)
    {
        return [.. eventEntries.Select(DeserializeEvent)];
    }

    private static IEvent DeserializeEvent(EventStoreEntry<TStreamIdentifier> entry)
    {
        var eventType = Type.GetType(entry.EventType)
            ?? throw new InvalidOperationException($"Event type '{entry.EventType}' not found.");

        return (IEvent)JsonSerializer.Deserialize(entry.Event, eventType)!;
    }

    public async Task SaveEventsAsync(IEnumerable<IEvent> events, TStreamIdentifier id, string streamType)
    {
        var eventEntries = CreateEventEntries(events, id, streamType);
        if (eventEntries.Count > 0)
        {
            await _repository.AddManyAsync(eventEntries);
            await _repository.SaveChangesAsync();
        }
    }

    private static List<EventStoreEntry<TStreamIdentifier>> CreateEventEntries(
        IEnumerable<IEvent> events, TStreamIdentifier streamIdentifier, string streamType)
    {
        ArgumentNullException.ThrowIfNull(events, nameof(events));
        ArgumentNullException.ThrowIfNull(streamIdentifier, nameof(streamIdentifier));
        ArgumentException.ThrowIfNullOrWhiteSpace(streamType, nameof(streamType));

        return events.Any() ? [.. events.Select(e => CreateEventEntry(e, streamIdentifier, streamType))] : [];
    }

    private static EventStoreEntry<TStreamIdentifier> CreateEventEntry(
        IEvent @event, TStreamIdentifier streamIdentifier, string streamType)
    {
        return new EventStoreEntry<TStreamIdentifier>
        {
            StreamId = streamIdentifier,
            StreamType = streamType,
            EventType = @event.GetType().AssemblyQualifiedName!,
            Event = JsonSerializer.Serialize(@event, @event.GetType()),
            Version = @event.Version
        };
    }

    public void Dispose()
    {
        _repository.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _repository.DisposeAsync();
    }
}
