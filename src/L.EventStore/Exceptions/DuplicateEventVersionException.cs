namespace L.EventStore.Exceptions;

public sealed class DuplicateEventVersionException<TStreamId>(
    TStreamId streamId, string streamType, long version) 
    : Exception($"Event with version {version} already exists in stream {streamId} of type {streamType}")
    where TStreamId : IEquatable<TStreamId>, IComparable<TStreamId>;