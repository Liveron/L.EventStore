using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace L.EventStore.EntityFramework.PostgreSQL.FunctionalTests;

[Collection("Guid EventStore Collection")]
public sealed class GuidEventStoreRepositoryTests(GuidEventStoreFixture fixture)
{
    private readonly IEventStoreRepository<Guid> _repository = fixture.EventStoreRepository;
    private readonly DbContext _context = fixture.DbContext;

    [Fact]
    public async Task AddAsync_ShouldAddEventToStore()
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
    public async Task AddAsync_ShouldThrowConcurrencyException_WhenStreamIdAndVersionRepeated()
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
        await _repository.AddAsync(duplicateEntry);

        // Assert
        await Assert.ThrowsAnyAsync<ConcurrencyException>(async () =>
            await _repository.SaveChangesAsync());
    }

    [Fact]
    public async Task Add_ShouldAddEventToStore()
    {
        // Arrange
        var @event = new TestEvent("Test Data");
        var entry = CreateEventEntry(Guid.NewGuid(), "TestStream", @event);

        // Act
        _repository.Add(entry);
        await _repository.SaveChangesAsync();
        var savedEvents = await _repository.GetEventsAsync(entry.StreamId);

        // Assert
        var savedEvent = Assert.Single(savedEvents);
        Assert.Equal(entry.StreamId, savedEvent.StreamId);
        Assert.Equal(entry.StreamType, savedEvent.StreamType);
        Assert.Equal(entry.EventType, savedEvent.EventType);
        Assert.Equal(entry.Version, savedEvent.Version);
        Assert.Equal(@event, JsonSerializer.Deserialize(entry.Event, typeof(TestEvent)));
    }

    [Fact]
    public async Task Add_ShouldThrowConcurrencyException_WhenStreamIdAndVersionRepeated()
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
    public async Task AddManyAsync_ShouldThrowConcurrencyException_WhenStreamIdAndVersionRepeated()
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

    [Fact]
    public async Task GetStreamVersion_ShouldReturnCorrectVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new[]
        {
            CreateEventEntry(streamId, "TestStream", "Data1", 1),
            CreateEventEntry(streamId, "TestStream", "Data2", 2),
            CreateEventEntry(streamId, "TestStream", "Data3", 3)
        };
        await _repository.AddManyAsync(events);
        await _repository.SaveChangesAsync();

        // Act
        var version = await _repository.GetStreamVersion(streamId);

        // Assert
        Assert.Equal(3, version);
    }

    [Fact]
    public async Task GetStreamVersion_WithNonExistentStream_ShouldReturnZero()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var version = await _repository.GetStreamVersion(nonExistentId);

        // Assert
        Assert.Equal(0, version);
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
    public async Task GetEventsAsync_ShouldMaintainVersionOrder()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var streamType = "TestStream";
        var events = new[]
        {
            CreateEventEntry(streamId, streamType, "Data1", 1),
            CreateEventEntry(streamId, streamType, "Data2", 2),
            CreateEventEntry(streamId, streamType, "Data3", 3)
        };

        // Act
        await _repository.AddManyAsync(events);
        await _repository.SaveChangesAsync();
        var savedEvents = await _repository.GetEventsAsync(streamId);

        // Assert
        Assert.Equal(3, savedEvents.Count);
        Assert.Equal(1, savedEvents[0].Version);
        Assert.Equal(2, savedEvents[1].Version);
        Assert.Equal(3, savedEvents[2].Version);
    }

    [Fact]
    public async Task GetEventsAsync_WithNonExistentStream_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var events = await _repository.GetEventsAsync(nonExistentId);

        // Assert
        Assert.Empty(events);
    }

    private static EventEntry<Guid> CreateEventEntry(
        Guid streamId, string streamType, string data, long version = 1)
    {
        return new EventEntry<Guid>
        {
            StreamId = streamId,
            StreamType = streamType,
            EventType = typeof(TestEvent).AssemblyQualifiedName!,
            Event = JsonSerializer.Serialize(new TestEvent(data)),
            Version = version
        };
    }

    private static EventEntry<Guid> CreateEventEntry(
        Guid streamId, string streamType, TestEvent @event, long version = 1)
    {
        return new EventEntry<Guid>
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
