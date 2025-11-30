namespace UserEvents.Infra.Interfaces;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent eventData);
}