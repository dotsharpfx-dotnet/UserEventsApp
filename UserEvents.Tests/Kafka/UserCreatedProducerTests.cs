using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Confluent.Kafka;
using UserEvents.Infra.Kafka;
using UserEvents.Models.Config;
using UserEvents.Infra;

namespace UserEvents.Tests.Kafka;

public class UserCreatedProducerTests
{
    private readonly Mock<IProducer<string, UserCreatedEvent>> _mockProducer;
    private readonly Mock<ILogger<UserCreatedProducer<string, UserCreatedEvent>>> _mockLogger;
    private readonly IOptions<UserEventsConfig> _options;
    private readonly UserCreatedProducer<string, UserCreatedEvent> _producer;

    public UserCreatedProducerTests()
    {
        _mockProducer = new Mock<IProducer<string, UserCreatedEvent>>();
        _mockLogger = new Mock<ILogger<UserCreatedProducer<string, UserCreatedEvent>>>();

        _options = Options.Create(new UserEventsConfig
        {
            KafkaConfig = new KafkaConfig
            {
                Topic = "user-events",
                BootstrapServers = "localhost:9092",
                ConsumerGroupId = "test-group",
                SchemaRegistryUrl = "http://localhost:8081"
            }
        });

        _producer = new UserCreatedProducer<string, UserCreatedEvent>(
            _mockProducer.Object,
            _options,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ProduceAsync_WithValidEvent_ShouldSucceed()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, UserCreatedEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<DeliveryReport<string, UserCreatedEvent>>());

        // Act
        await _producer.ProduceAsync("user-123", userEvent);

        // Assert
        _mockProducer.Verify(
            p => p.ProduceAsync(
                "user-events",
                It.Is<Message<string, UserCreatedEvent>>(m => m.Key == "user-123" && m.Value == userEvent),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProduceAsync_WithNullEvent_ShouldThrowArgumentNullException()
    {
        // Arrange
        UserCreatedEvent nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _producer.ProduceAsync("user-123", nullEvent));
    }

    [Fact]
    public async Task ProduceAsync_WhenKafkaThrowsException_ShouldRethrow()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, UserCreatedEvent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProduceException<string, UserCreatedEvent>(
                new Error(ErrorCode.Unknown, "Test error"),
                null));

        // Act & Assert
        await Assert.ThrowsAsync<ProduceException<string, UserCreatedEvent>>(
            async () => await _producer.ProduceAsync("user-123", userEvent));
    }

    [Fact]
    public async Task ProduceAsync_ShouldUseConfiguredTopic()
    {
        // Arrange
        var userEvent = new UserCreatedEvent
        {
            UserId = "user-123",
            UserName = "john_doe",
            UserEmail = "john@example.com",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _mockProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, UserCreatedEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<DeliveryReport<string, UserCreatedEvent>>());

        // Act
        await _producer.ProduceAsync("user-123", userEvent);

        // Assert
        _mockProducer.Verify(
            p => p.ProduceAsync("user-events", It.IsAny<Message<string, UserCreatedEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
