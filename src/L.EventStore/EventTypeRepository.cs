using L.EventStore.Abstractions;

namespace L.EventStore;

public sealed class EventTypeRepository : IEventTypeRepository
{
    private readonly Dictionary<string, Type> _eventTypes = [];

    public void AddEventTypeIfNotExist(string typeName, Type eventType)
    {
        if (!_eventTypes.ContainsKey(typeName))
        {
            _eventTypes[typeName] = eventType;
        }
    }

    public Type? GetEventType(string typeName)
    {
        return _eventTypes.GetValueOrDefault(typeName);
    }
}
