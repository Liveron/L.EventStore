using L.EventStore.Projector.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace L.EventStore.Projector.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddEventStore_ShouldRegisterGuidEventStore_WhenStreamIdNotPresent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEventStoreProjector();
        var provider = services.BuildServiceProvider();

        // Assert
        var
    }
}
