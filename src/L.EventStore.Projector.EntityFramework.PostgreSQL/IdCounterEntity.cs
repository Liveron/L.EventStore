namespace L.EventStore.Projector.EntityFramework.PostgreSQL;

public sealed class IdCounterEntity
{
    public long Id { get; set; }
    public DateTime LastProcessedAt { get; set; }
    public long LastProcessedId { get; set; }
}
