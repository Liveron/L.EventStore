using L.EventStore.Abstractions;
using L.EventStore.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace L.EventStore.EntityFramework.FunctionalTests;

public sealed class GuidEventStoreRepositoryTests(GuidEventStoreRepositoryFixture fixture) 
    : IClassFixture<GuidEventStoreRepositoryFixture>
{
    private readonly DbContext _context = fixture.Context;
    private readonly IEventStoreRepository<Guid> _repository = fixture.Repository;

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
    public async Task GetEventsAsync_ShouldReturnEventsInOrder()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new[]
        {
            CreateEventEntry(streamId, "TestStream", "Data3", 3),
            CreateEventEntry(streamId, "TestStream", "Data1", 1),
            CreateEventEntry(streamId, "TestStream", "Data2", 2)
        };

        // Act
        await _repository.AddManyAsync(events);
        await _repository.SaveChangesAsync();
        var result = await _repository.GetEventsAsync(streamId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].Version);
        Assert.Equal(2, result[1].Version);
        Assert.Equal(3, result[2].Version);
    }

    [Fact]
    public async Task GetEventsAsync_WithNonExistentStream_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetEventsAsync(nonExistentId);

        // Assert
        Assert.Empty(result);
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
