namespace L.EventStore.Abstractions;

public interface IEventStore<TStreamIdentifier> : IDisposable, IAsyncDisposable
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    Task SaveEventsAsync(IEnumerable<IEvent> events, TStreamIdentifier id, string streamType);
    Task<List<IEvent>> GetEventStreamAsync(TStreamIdentifier id);
    Task<List<IEvent>> GetEventStreamAsync(TStreamIdentifier id, string streamType);
}
