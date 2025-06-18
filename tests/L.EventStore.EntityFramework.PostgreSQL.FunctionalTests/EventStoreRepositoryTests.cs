using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[Collection("PostgreSQL EventStore Collection")]
public sealed class EventStoreRepositoryTests(PostgreEventStoreFixture fixture)
{
    private readonly IEventStoreRepository<Guid> _repository = fixture.Repository;
    private readonly DbContext _context = fixture.DbContext;

    [Fact]
    public async Task AddAsync_ShouldAddEntryToStore()
    {
        // Arrange
        var @event = new TestEvent("Test Data");
        var entry = CreateEventEntry(Guid.NewGuid(), "TestStream", @event);

        // Act
        await _repository.AddAsync(entry);
        await _repository.SaveChangesAsync();
        var savedEvents = await _repository.GetEventsAsync(entry.StreamId);

        // Assert
        var savedEvent = Assert.Single(savedEvents);
        Assert.Equal(entry.StreamId, savedEvent.StreamId);
        Assert.Equal(entry.StreamType, savedEvent.StreamType);
        Assert.Equal(entry.EventType, savedEvent.EventType);
        Assert.Equal(entry.Version, savedEvent.Version);
        Assert.Equal(@event, JsonSerializer.Deserialize(savedEvent.Event, typeof(TestEvent)));
    }

    [Fact]
    public async Task AddManyAsync_ShouldAddMultipleEntries()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new[]
        {
            CreateEventEntry(streamId, "TestStream", "Data1", 1),
            CreateEventEntry(streamId, "TestStream", "Data2", 2),
            CreateEventEntry(streamId, "TestStream", "Data3", 3)
        };

        // Act
        await _repository.AddManyAsync(events);
        await _repository.SaveChangesAsync();
        var savedEvents = await _repository.GetEventsAsync(streamId);

        // Assert
        Assert.Equal(3, savedEvents.Count);
        Assert.All(savedEvents, e => Assert.Equal(streamId, e.StreamId));
    }

    [Fact]
    public async Task GetEventsAsync_ShouldReturnEntriesForStreamIdAndType()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var streamType = "TestStream";
        var anotherType = "AnotherStreamType";
        var events = new[]
        {
            CreateEventEntry(streamId, streamType, "Data1", 1),
            CreateEventEntry(streamId, streamType, "Data2", 2),
            CreateEventEntry(streamId, anotherType, "Data3", 3),
            CreateEventEntry(streamId, anotherType, "Data4", 4),
            CreateEventEntry(streamId, anotherType, "Data5", 5)
        };

        // Act
        await _repository.AddManyAsync(events);
        await _repository.SaveChangesAsync();
        var savedEvents = await _repository.GetEventsAsync(streamId, streamType);

        // Assert
        Assert.Equal(2, savedEvents.Count);
        Assert.All(savedEvents, e => Assert.Equal(streamId, e.StreamId));
        Assert.All(savedEvents, e => Assert.Equal(streamType, e.StreamType));
    }

    [Fact]
    public async Task Add_ShouldThrowConcurrencyException_WhenStreamIdAndTypeAndVersionRepeated()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var streamType = "TestStream";
        var eventEntries = new[]
        {
            CreateEventEntry(streamId, streamType, "Data1", 1),
            CreateEventEntry(streamId, streamType, "Data2", 2),
        };
        var duplicateEntry = CreateEventEntry(streamId, streamType, "Data3", 2); 

        // Act
        await _repository.AddManyAsync(eventEntries);
        await _repository.SaveChangesAsync();
        _context.ChangeTracker.Clear(); // Clear the context to avoid tracking issues
        _repository.Add(duplicateEntry);

        // Assert
        await Assert.ThrowsAnyAsync<ConcurrencyException>(async () =>
            await _repository.SaveChangesAsync());
    }

    [Fact]
    public async Task AddManyAsync_ShouldThrowConcurrencyException_WhenStreamIdAndTypeAndVersionRepeated()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var streamType = "TestStream";
        var eventEntries = new[]
        {
            CreateEventEntry(streamId, streamType, "Data1", 1),
            CreateEventEntry(streamId, streamType, "Data2", 2),
        };
        var entriesWithRepeated = new[]
        {
            CreateEventEntry(streamId, streamType, "Data3", 1),
            CreateEventEntry(streamId, streamType, "Data4", 2)
        };

        // Act
        await _repository.AddManyAsync(eventEntries);
        await _repository.SaveChangesAsync();
        _context.ChangeTracker.Clear();
        await _repository.AddManyAsync(entriesWithRepeated);

        // Assert
        await Assert.ThrowsAnyAsync<ConcurrencyException>(async () =>
            await _repository.SaveChangesAsync());
    }

    private static EventStoreEntry<Guid> CreateEventEntry(
        Guid streamId, string streamType, string data, long version = 1)
    {
        return new EventStoreEntry<Guid>
        {
            StreamId = streamId,
            StreamType = streamType,
            EventType = typeof(TestEvent).AssemblyQualifiedName!,
            Event = JsonSerializer.Serialize(new TestEvent(data)),
            Version = version
        };
    }

    private static EventStoreEntry<Guid> CreateEventEntry(
        Guid streamId, string streamType, TestEvent @event, long version = 1)
    {
        return new EventStoreEntry<Guid>
        {
            StreamId = streamId,
            StreamType = streamType,
            EventType = typeof(TestEvent).AssemblyQualifiedName!,
            Event = JsonSerializer.Serialize(@event),
            Version = version
        };
    }

    private sealed record TestEvent(string Data);
}
