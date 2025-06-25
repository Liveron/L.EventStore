using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using System.Text.Json;

namespace L.EventStore;

public sealed class EventStore<TStreamIdentifier, TEvent>(
    IEventStoreRepository<TStreamIdentifier> repository)
    : IEventStore<TStreamIdentifier, TEvent>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier> 
    where TEvent : notnull
{
    private readonly IEventStoreRepository<TStreamIdentifier> _repository = repository 
        ?? throw new ArgumentNullException(nameof(repository));

    public async Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(streamType);

        var eventEntries = await _repository.GetEventsAsync(id, streamType);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    public async Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var eventEntries = await _repository.GetEventsAsync(id);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    private static List<TEvent> DeserializeEvents(List<EventStoreEntry<TStreamIdentifier>> eventEntries)
    {
        return [.. eventEntries.Select(DeserializeEvent)];
    }

    private static TEvent DeserializeEvent(EventStoreEntry<TStreamIdentifier> entry)
    {
        var eventType = Type.GetType(entry.EventType)
            ?? throw new InvalidOperationException($"Event type '{entry.EventType}' not found.");

        return (TEvent)JsonSerializer.Deserialize(entry.Event, eventType)!;
    }

    public async Task SaveEventsAsync(IEnumerable<TEvent> events, TStreamIdentifier streamIdentifier,
        string streamType, long expectedVersion = 0)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(streamIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(streamType);

        if (expectedVersion < 0)
            throw new InvalidOperationException("Expected version cannot be negative.");

        if (!events.Any())
            return;

        var currentVersion = await _repository.GetStreamVersion(streamIdentifier);

        if (currentVersion != expectedVersion)
            throw new ConcurrencyException();

        var eventEntries = CreateEventEntries(events, streamIdentifier, streamType, expectedVersion);
        await _repository.AddManyAsync(eventEntries);
        await _repository.SaveChangesAsync();
    }

    private static List<EventStoreEntry<TStreamIdentifier>> CreateEventEntries(
        IEnumerable<TEvent> events, TStreamIdentifier streamIdentifier, string streamType, long expectedVersion)
    {
        var eventEntries = new List<EventStoreEntry<TStreamIdentifier>>();

        var version = expectedVersion;
        foreach (var @event in events)
        {
            version++;
            eventEntries.Add(CreateEventEntry(@event, streamIdentifier, streamType, version));
        }

        return eventEntries;
    }

    private static EventStoreEntry<TStreamIdentifier> CreateEventEntry(
        TEvent @event, TStreamIdentifier streamIdentifier, string streamType, long version)
    {
        return new EventStoreEntry<TStreamIdentifier>
        {
            StreamId = streamIdentifier,
            StreamType = streamType,
            EventType = @event.GetType().AssemblyQualifiedName!,
            Event = JsonSerializer.Serialize(@event, @event.GetType()),
            Version = version,
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
