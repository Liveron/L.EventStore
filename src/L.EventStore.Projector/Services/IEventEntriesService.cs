using L.EventStore.Abstractions;

namespace L.EventStore.Projector.Services;

internal interface IEventEntriesService<TStreamIdentifier>
    where TStreamIdentifier : IComparable<TStreamIdentifier>, IEquatable<TStreamIdentifier>
{
    Task<List<EventEntry<TStreamIdentifier>>> GetUnprocessedEventEntriesAsync();
}
