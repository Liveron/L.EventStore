namespace L.EventStore.Abstractions;

public interface IEventStore<TStreamIdentifier, TEvent>: IEventStore<TStreamIdentifier>
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
    where TEvent : notnull
{
    Task SaveEventsAsync(IEnumerable<TEvent> events, EventStreamOptions<TStreamIdentifier> options);
    Task IEventStore<TStreamIdentifier>.SaveEventsAsync(
        IEnumerable<object> events, EventStreamOptions<TStreamIdentifier> options) =>
        SaveEventsAsync(events.Cast<TEvent>(), options);

    new Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id);
    Task<List<object>> IEventStore<TStreamIdentifier>.GetEventStreamAsync(TStreamIdentifier id) =>
        GetEventStreamAsync(id).ContinueWith(events => events.Result.Cast<object>().ToList());

    new Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType);
    Task<List<object>> IEventStore<TStreamIdentifier>.GetEventStreamAsync(TStreamIdentifier id, string streamType) =>
        GetEventStreamAsync(id, streamType).ContinueWith(events => events.Result.Cast<object>().ToList());
}

public interface IEventStore<TStreamIdentifier> : IDisposable, IAsyncDisposable
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    Task SaveEventsAsync(IEnumerable<object> events, EventStreamOptions<TStreamIdentifier> options);
    Task<List<object>> GetEventStreamAsync(TStreamIdentifier id);
    Task<List<object>> GetEventStreamAsync(TStreamIdentifier id, string streamType);
}