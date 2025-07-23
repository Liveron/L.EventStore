namespace L.EventStore.Abstractions;

public interface IEventTypeRepository
{
    public void AddEventTypeIfNotExist(string typeName, Type eventType);
    public Type? GetEventType(string typeName);
}
