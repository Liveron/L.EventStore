using L.EventStore.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.PostgreSQL.DataModel;

public static class ModelBuilderExtensions
{
    public static void AddEventStore<TEventStreamId>(this ModelBuilder modelBuilder, 
        string tableName = "event_store")
        where TEventStreamId : IEquatable<TEventStreamId>, IComparable<TEventStreamId>
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        modelBuilder.Entity<EventEntry<TEventStreamId>>(entity =>
        {
            entity.ToTable(tableName);

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.StreamId, e.Version })
                .IsUnique();

            entity.Property(e => e.Event)
                .HasColumnType("jsonb");
        });
    }
}
