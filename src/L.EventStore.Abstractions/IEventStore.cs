namespace L.EventStore.Abstractions;

public interface IEventStore<TStreamIdentifier, TEvent> : IDisposable, IAsyncDisposable
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
    where TEvent : notnull
{
    Task SaveEventsAsync(IEnumerable<TEvent> events, TStreamIdentifier id, string streamType, long expectedVersion = 0);
    Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id);
    Task<List<TEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType);
}