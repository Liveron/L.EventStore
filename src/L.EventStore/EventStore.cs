using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using System.Text.Json;

namespace L.EventStore;

public abstract class EventStoreBase<TStreamIdentifier>(
    IEventStoreRepository<TStreamIdentifier> repository, IEventTypeRepository eventTypeRepository)
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    protected readonly IEventStoreRepository<TStreamIdentifier> _repository = repository
        ?? throw new ArgumentNullException(nameof(repository));

    protected readonly IEventTypeRepository _eventTypeRepository = eventTypeRepository
        ?? throw new ArgumentNullException(nameof(eventTypeRepository));

    protected List<TEvent> DeserializeEvents<TEvent>(IEnumerable<EventEntry<TStreamIdentifier>> eventEntries)
    {
        return [.. eventEntries.Select(DeserializeEvent<TEvent>)];
    }

    protected List<object> DeserializeEvents(IEnumerable<EventEntry<TStreamIdentifier>> eventEntries)
    {
        return [.. eventEntries.Select(DeserializeEvent)];
    }
   
    protected object DeserializeEvent(EventEntry<TStreamIdentifier> entry)
    {
        var eventType = GetEventTypeAndThrowIfEventNotFound(entry.EventType);

        return JsonSerializer.Deserialize(entry.Event, eventType)!;
    }

    protected TEvent DeserializeEvent<TEvent>(EventEntry<TStreamIdentifier> entry)
    {
        var eventType = GetEventTypeAndThrowIfEventNotFound(entry.EventType);

        return (TEvent)JsonSerializer.Deserialize(entry.Event, eventType)!;
    }

    private Type GetEventTypeAndThrowIfEventNotFound(string eventType)
    {
        return _eventTypeRepository.GetEventType(eventType) ??
            throw new InvalidOperationException($"Event type '{eventType}' not found.");
    }

    protected static List<EventEntry<TStreamIdentifier>> CreateEventEntries<TEvent>(
        IEnumerable<TEvent> events, TStreamIdentifier streamIdentifier, string streamType, long expectedVersion)
        where TEvent : notnull
    {
        var eventEntries = new List<EventEntry<TStreamIdentifier>>();

        var version = expectedVersion;
        foreach (var @event in events)
        {
            version++;
            eventEntries.Add(CreateEventEntry(@event, streamIdentifier, streamType, version));
        }

        return eventEntries;
    }

    private static EventEntry<TStreamIdentifier> CreateEventEntry<TEvent>(
        TEvent @event, TStreamIdentifier streamIdentifier, string streamType, long version)
        where TEvent : notnull
    {
        return new EventEntry<TStreamIdentifier>
        {
            StreamId = streamIdentifier,
            StreamType = streamType,
            EventType = @event.GetType().Name,
            Event = JsonSerializer.Serialize(@event),
            Version = version,
        };
    }
}

public sealed class EventStore<TStreamIdentifier, TEvent>(
    IEventStoreRepository<TStreamIdentifier> repository, IEventTypeRepository eventTypeRepository)
    : EventStoreBase<TStreamIdentifier>(repository, eventTypeRepository), IEventStore<TStreamIdentifier, TEvent>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier> 
    where TEvent : notnull
{
    public async Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamType);

        var eventEntries = await _repository.GetEventsAsync(id, streamType);
        return eventEntries.Count == 0 ? [] : DeserializeEvents<TEvent>(eventEntries);
    }

    public async Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id)
    {
        var eventEntries = await _repository.GetEventsAsync(id);
        return eventEntries.Count == 0 ? [] : DeserializeEvents<TEvent>(eventEntries);
    }

    public async Task SaveEventsAsync(IEnumerable<TEvent> events, EventStreamOptions<TStreamIdentifier> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.StreamType);

        if (options.ExpectedVersion < 0)
            throw new InvalidOperationException("Expected version cannot be negative.");

        if (!events.Any())
            return;

        var currentVersion = await _repository.GetStreamVersion(options.StreamId);

        if (currentVersion != options.ExpectedVersion)
            throw new ConcurrencyException();

        var eventEntries = CreateEventEntries(events, options.StreamId, options.StreamType, options.ExpectedVersion);
        await _repository.AddManyAsync(eventEntries);
        await _repository.SaveChangesAsync();
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

public sealed class EventStore<TStreamIdentifier>(
    IEventStoreRepository<TStreamIdentifier> repository, IEventTypeRepository eventTypeRepository) 
    : EventStoreBase<TStreamIdentifier>(repository, eventTypeRepository), IEventStore<TStreamIdentifier>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    public async Task<List<object>> GetEventStreamAsync(TStreamIdentifier id, string streamType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamType);

        var eventEntries = await _repository.GetEventsAsync(id, streamType);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    public async Task<List<object>> GetEventStreamAsync(TStreamIdentifier id)
    {
        var eventEntries = await _repository.GetEventsAsync(id);
        return eventEntries.Count == 0 ? [] : DeserializeEvents(eventEntries);
    }

    public async Task SaveEventsAsync(IEnumerable<object> events, EventStreamOptions<TStreamIdentifier> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.StreamType);

        if (options.ExpectedVersion < 0)
            throw new InvalidOperationException("Expected version cannot be negative.");

        if (!events.Any())
            return;

        var currentVersion = await _repository.GetStreamVersion(options.StreamId);

        if (currentVersion != options.ExpectedVersion)
            throw new ConcurrencyException();

        var eventEntries = CreateEventEntries(events, options.StreamId, options.StreamType, options.ExpectedVersion);
        await _repository.AddManyAsync(eventEntries);
        await _repository.SaveChangesAsync();
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