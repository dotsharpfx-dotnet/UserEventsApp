using UserEvents.Infra.Interfaces;

namespace UserEvents.Infra.Kafka;

public class UserCreationHandler : IEventHandler<UserCreatedEvent>
{
    public Task HandleAsync(UserCreatedEvent @event)
    {
        // Handle the UserCreatedEvent (e.g., log it, process it, etc.)
        Console.WriteLine($"User created: {@event.UserId}, Email: {@event.UserEmail}");
        return Task.CompletedTask;
    }
}