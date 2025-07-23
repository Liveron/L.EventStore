namespace L.EventStore.Projector.Abstractions;

public interface IEventHandler<TEvent> : IEventHandler
{
    Task HandleAsync(TEvent @event);
    Task IEventHandler.HandleAsync(object @event) => HandleAsync((TEvent)@event);
}

public interface IEventHandler
{
    Task HandleAsync(object @event);
}