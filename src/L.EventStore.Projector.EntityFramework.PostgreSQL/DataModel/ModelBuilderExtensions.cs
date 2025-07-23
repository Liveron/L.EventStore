using Microsoft.EntityFrameworkCore;

namespace L.EventStore.Projector.EntityFramework.PostgreSQL.DataModel;

public static class ModelBuilderExtensions
{
    public static void AddProjectorDateCounter(this ModelBuilder modelBuilder,
        string dateCounterTableName = "id_counter")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dateCounterTableName);

        modelBuilder.Entity<IdCounterEntity>(entity =>
        {
            entity.ToTable(dateCounterTableName);

            entity.HasKey(e => e.Id);
        });
    }
}
