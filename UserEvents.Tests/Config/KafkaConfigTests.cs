using Xunit;
using Microsoft.Extensions.Options;
using UserEvents.Models.Config;

namespace UserEvents.Tests.Config;

public class KafkaConfigTests
{
    [Fact]
    public void KafkaConfig_WithValidSettings_ShouldCreateSuccessfully()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        var topic = "user-events";
        var consumerGroupId = "user-events-consumer-group";
        var schemaRegistryUrl = "http://localhost:8081";

        // Act
        var config = new KafkaConfig
        {
            BootstrapServers = bootstrapServers,
            Topic = topic,
            ConsumerGroupId = consumerGroupId,
            SchemaRegistryUrl = schemaRegistryUrl
        };

        // Assert
        Assert.Equal(bootstrapServers, config.BootstrapServers);
        Assert.Equal(topic, config.Topic);
        Assert.Equal(consumerGroupId, config.ConsumerGroupId);
        Assert.Equal(schemaRegistryUrl, config.SchemaRegistryUrl);
    }

    [Fact]
    public void KafkaConfig_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var config = new KafkaConfig();

        // Assert
        Assert.Equal("localhost:9092", config.BootstrapServers);
        Assert.Equal("user-events", config.Topic);
        Assert.Equal("http://localhost:8081", config.SchemaRegistryUrl);
    }

    [Fact]
    public void KafkaConfig_WithMultipleBootstrapServers_ShouldBeSupported()
    {
        // Arrange
        var bootstrapServers = "kafka-1:9092,kafka-2:9092,kafka-3:9092";

        // Act
        var config = new KafkaConfig
        {
            BootstrapServers = bootstrapServers,
            Topic = "user-events",
            ConsumerGroupId = "test-group",
            SchemaRegistryUrl = "http://localhost:8081"
        };

        // Assert
        Assert.Equal(bootstrapServers, config.BootstrapServers);
        Assert.Contains("kafka-1:9092", config.BootstrapServers);
        Assert.Contains("kafka-2:9092", config.BootstrapServers);
        Assert.Contains("kafka-3:9092", config.BootstrapServers);
    }

    [Fact]
    public void KafkaConfig_CanBeConfiguredViaOptions()
    {
        // Arrange
        var kafkaConfig = new KafkaConfig
        {
            BootstrapServers = "kafka:9092",
            Topic = "test-topic",
            ConsumerGroupId = "test-group",
            SchemaRegistryUrl = "http://schema-registry:8081"
        };
        var options = Options.Create(kafkaConfig);

        // Act & Assert
        Assert.NotNull(options.Value);
        Assert.Equal("kafka:9092", options.Value.BootstrapServers);
        Assert.Equal("test-topic", options.Value.Topic);
    }

    [Fact]
    public void UserEventsConfig_WithKafkaConfig_ShouldCreateSuccessfully()
    {
        // Arrange
        var kafkaConfig = new KafkaConfig
        {
            BootstrapServers = "localhost:9092",
            Topic = "user-events",
            ConsumerGroupId = "test-group",
            SchemaRegistryUrl = "http://localhost:8081"
        };

        // Act
        var userEventsConfig = new UserEventsConfig
        {
            KafkaConfig = kafkaConfig
        };

        // Assert
        Assert.NotNull(userEventsConfig.KafkaConfig);
        Assert.Equal("localhost:9092", userEventsConfig.KafkaConfig.BootstrapServers);
        Assert.Equal("user-events", userEventsConfig.KafkaConfig.Topic);
    }

    [Fact]
    public void KafkaConfig_SchemaRegistryUrl_ShouldSupportHttpsProtocol()
    {
        // Arrange
        var schemaRegistryUrl = "https://schema-registry.prod.example.com:8081";

        // Act
        var config = new KafkaConfig
        {
            BootstrapServers = "localhost:9092",
            Topic = "user-events",
            ConsumerGroupId = "test-group",
            SchemaRegistryUrl = schemaRegistryUrl
        };

        // Assert
        Assert.StartsWith("https://", config.SchemaRegistryUrl);
        Assert.Equal(schemaRegistryUrl, config.SchemaRegistryUrl);
    }
}
