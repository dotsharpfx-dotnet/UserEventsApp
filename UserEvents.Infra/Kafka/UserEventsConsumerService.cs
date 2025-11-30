using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEvents.Infra.Interfaces;
using UserEvents.Models.Config;

namespace UserEvents.Infra.Kafka;

public class UserEventsConsumerService(
    IConsumer<string, UserCreatedEvent> consumer,
    IEventHandler<UserCreatedEvent> userCreatedEventHandler,
    IEventProducer<string, UserCreatedEvent> producer,
    ILogger<UserEventsConsumerService> logger,
    IOptions<UserEventsConfig> options) : BackgroundService
{
    private readonly string topic = options.Value.KafkaConfig.Topic;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var userId = "vk260";
        var username = "vijay-karajgikar";
        var userEmail = "dotsharpfx@gmail.com";
        var createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await producer.ProduceAsync(userId, new UserCreatedEvent(userId, username, userEmail, createdAt));

        await StartConsumerLoop(stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken stoppingToken)
    {
        consumer.Subscribe(topic);
        logger.LogInformation("Subscribed to topic: {Topic}", topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    var userEvent = consumeResult.Message.Value;
                    await userCreatedEventHandler.HandleAsync(userEvent);
                    logger.LogInformation("Consumed message with key: {Key}, UserId: {UserId}", consumeResult.Message.Key, userEvent.UserId);
                    consumer.Commit(consumeResult);

                    var userId = $"vk260-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                    var username = "vijay-karajgikar";
                    var userEmail = "dotsharpfx@gmail.com";
                    var createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await producer.ProduceAsync(userId, new UserCreatedEvent(userId, username, userEmail, createdAt));
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Error occurred while consuming message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Consumer loop cancelled");
        }
        finally
        {
            consumer.Close();
            logger.LogInformation("Consumer closed");
        }
    }
}
