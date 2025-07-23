using L.EventStore.Abstractions;

namespace L.EventStore.Projector.Services;

public interface IProjectorService<TStreamIdentifier> where TStreamIdentifier 
    : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    Task ProjectAsync(List<EventEntry<TStreamIdentifier>> eventEntries);
}
