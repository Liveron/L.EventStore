using L.EventStore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.PostgreSQL.DataModel;

public static class ModelBuilderExtensions
{
    public static void AddEventStore<TEventStreamId>(this ModelBuilder modelBuilder,
        string tableName = "event_store")
        where TEventStreamId : IEquatable<TEventStreamId>, IComparable<TEventStreamId>
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

        modelBuilder.Entity<EventStoreEntry<TEventStreamId>>(entity =>
        {
            entity.ToTable(tableName);

            entity.HasKey(e => new { e.StreamId, e.StreamType, e.Version });

            entity.Property(e => e.Event)
                .HasColumnType("jsonb");
        });
    }
}
