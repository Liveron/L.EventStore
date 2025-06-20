using dotenv.net;
using L.EventStore.Abstractions;
using L.EventStore.EntityFramework.PostgreSQL.DataModel;
using Microsoft.EntityFrameworkCore;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[CollectionDefinition("PostgreSQL EventStore Collection")]
public sealed class PostgreEventStoreCollection : ICollectionFixture<PostgreEventStoreFixture>;

public sealed class PostgreEventStoreFixture : IDisposable
{
    private const string _environmentEVN = "DOTNET_ENVIRONMENT";
    private const string _connectionStringEVN = "POSTGRESQL_DB";

    public TestDbContext DbContext { get; }
    public IEventStoreRepository<Guid> Repository { get; }
    public IEventStore<Guid> EventStore { get; }

    public PostgreEventStoreFixture()
    {
        var environment = Environment.GetEnvironmentVariable(_environmentEVN)
            ?? throw new InvalidOperationException($"Environment variable {_environmentEVN} isn't set");

        if (environment == "Development")
            DotEnv.Load();

        var connectionString = Environment.GetEnvironmentVariable(_connectionStringEVN)
            ?? throw new InvalidOperationException($"Environment variable {_connectionStringEVN} isn't set");

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        DbContext = new TestDbContext(options);
        DbContext.Database.EnsureCreated();

        Repository = new EventStoreRepository<TestDbContext, Guid>(DbContext);
        EventStore = new EventStore<Guid>(Repository);
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        EventStore.Dispose();
    }
}

public sealed class TestDbContext(DbContextOptions<TestDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddEventStore<Guid>("event_store_test");
    }
}

