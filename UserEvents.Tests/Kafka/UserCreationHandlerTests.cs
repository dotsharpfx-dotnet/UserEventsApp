using Xunit;
using UserEvents.Infra;
using UserEvents.Infra.Kafka;

namespace UserEvents.Tests.Kafka;

public class UserCreationHandlerTests
{
    private readonly UserCreationHandler _handler;

    public UserCreationHandlerTests()
    {
        _handler = new UserCreationHandler();
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldComplete()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // Act
        await _handler.HandleAsync(userEvent);

        // Assert - Verify no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task HandleAsync_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserCreatedEvent nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _handler.HandleAsync(nullEvent));
    }

    [Fact]
    public async Task HandleAsync_WithMultipleEvents_ShouldHandleAllSuccessfully()
    {
        // Arrange
        var events = new List<UserCreatedEvent>
        {
            new UserCreatedEvent { UserId = "user-1", UserName = "user1", UserEmail = "user1@example.com", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            new UserCreatedEvent { UserId = "user-2", UserName = "user2", UserEmail = "user2@example.com", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            new UserCreatedEvent { UserId = "user-3", UserName = "user3", UserEmail = "user3@example.com", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
        };

        // Act
        foreach (var @event in events)
        {
            await _handler.HandleAsync(@event);
        }

        // Assert - Verify no exception was thrown
        Assert.True(true);
    }

    [Fact]
    public async Task HandleAsync_WithValidUserData_ShouldPreserveAllFields()
    {
        // Arrange
        var userId = "user-test-123";
        var username = "test_user";
        var email = "test@example.com";
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var userEvent = new UserCreatedEvent
        {
            UserId = userId,
            UserName = username,
            UserEmail = email,
            CreatedAt = createdAt
        };

        // Act
        await _handler.HandleAsync(userEvent);

        // Assert
        Assert.Equal(userId, userEvent.UserId);
        Assert.Equal(username, userEvent.UserName);
        Assert.Equal(email, userEvent.UserEmail);
        Assert.Equal(createdAt, userEvent.CreatedAt);
    }
}
