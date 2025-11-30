using Xunit;
using Avro;
using Avro.Specific;
using UserEvents.Infra;

namespace UserEvents.Tests.Models;

public class UserCreatedEventTests
{
    [Fact]
    public void UserCreatedEvent_WithAllProperties_ShouldCreateSuccessfully()
    {
        // Arrange
        var userId = "user-123";
        var username = "john_doe";
        var email = "john@example.com";
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        var userEvent = new UserCreatedEvent
        {
            UserId = userId,
            UserName = username,
            UserEmail = email,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(userId, userEvent.UserId);
        Assert.Equal(username, userEvent.UserName);
        Assert.Equal(email, userEvent.UserEmail);
        Assert.Equal(createdAt, userEvent.CreatedAt);
    }

    [Fact]
    public void UserCreatedEvent_ImplementsISpecificRecord()
    {
        // Arrange & Act
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = 1234567890000
        };

        // Assert
        Assert.IsAssignableFrom<ISpecificRecord>(userEvent);
    }

    [Fact]
    public void UserCreatedEvent_Get_WithValidFieldPosition_ShouldReturnCorrectValue()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = 1234567890000
        };

        // Act & Assert
        Assert.Equal("user-123", userEvent.Get(0));
        Assert.Equal("john_doe", userEvent.Get(1));
        Assert.Equal("john@example.com", userEvent.Get(2));
        Assert.Equal(1234567890000L, userEvent.Get(3));
    }

    [Fact]
    public void UserCreatedEvent_Get_WithInvalidFieldPosition_ShouldThrowException()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = 1234567890000
        };

        // Act & Assert
        Assert.Throws<AvroRuntimeException>(() => userEvent.Get(99));
    }

    [Fact]
    public void UserCreatedEvent_Put_WithValidFieldPosition_ShouldSetCorrectValue()
    {
        // Arrange
        var userEvent = new UserCreatedEvent();

        // Act
        userEvent.Put(0, "user-456");
        userEvent.Put(1, "jane_doe");
        userEvent.Put(2, "jane@example.com");
        userEvent.Put(3, 9876543210000L);

        // Assert
        Assert.Equal("user-456", userEvent.UserId);
        Assert.Equal("jane_doe", userEvent.UserName);
        Assert.Equal("jane@example.com", userEvent.UserEmail);
        Assert.Equal(9876543210000L, userEvent.CreatedAt);
    }

    [Fact]
    public void UserCreatedEvent_Put_WithInvalidFieldPosition_ShouldThrowException()
    {
        // Arrange
        var userEvent = new UserCreatedEvent();

        // Act & Assert
        Assert.Throws<AvroRuntimeException>(() => userEvent.Put(99, "value"));
    }

    [Fact]
    public void UserCreatedEvent_EmailValidation_ShouldAcceptValidEmails()
    {
        // Arrange & Act
        var validEmails = new[]
        {
            "user@example.com",
            "john.doe@company.co.uk",
            "test+tag@domain.org"
        };

        // Act & Assert
        foreach (var email in validEmails)
        {
            var userEvent = new UserCreatedEvent
            {
                UserId = "user-123",
                UserName = "testuser",
                UserEmail = email,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            Assert.Equal(email, userEvent.UserEmail);
        }
    }

    [Fact]
    public void UserCreatedEvent_CreatedAt_ShouldStoreUnixTimestamp()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

        // Act
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = timestamp
        };

        // Assert
        Assert.Equal(timestamp, userEvent.CreatedAt);
        Assert.True(userEvent.CreatedAt > 0);
    }

    [Fact]
    public void UserCreatedEvent_PropertiesAreModifiable()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = 1234567890000
        };

        // Act
        userEvent.UserId = "user-456";
        userEvent.UserName = "jane_doe";
        userEvent.UserEmail = "jane@example.com";

        // Assert
        Assert.Equal("user-456", userEvent.UserId);
        Assert.Equal("jane_doe", userEvent.UserName);
        Assert.Equal("jane@example.com", userEvent.UserEmail);
    }
}
