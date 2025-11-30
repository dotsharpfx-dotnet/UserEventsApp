using System.Collections.Concurrent;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEvents.Infra.Interfaces;
using UserEvents.Models.Config;

namespace UserEvents.Infra.Kafka;

public class UserCreatedProducer<TKey, TValue>(
    IProducer<TKey, TValue> producer,
    IOptions<UserEventsConfig> options,
    ILogger<UserCreatedProducer<TKey, TValue>> logger) : IEventProducer<TKey, TValue> where TValue : class
{
    private readonly string topic = options.Value.KafkaConfig.Topic;
    public async Task ProduceAsync(TKey key, TValue value)
    {
        try
        {
            var message = new Message<TKey, TValue> { Key = key, Value = value };
            var deliveryResult = await producer.ProduceAsync(topic, message);
            
            logger.LogInformation(
                "Produced event {EventType} to topic '{Topic}', Partition: {Partition}, Offset: {Offset}",
                typeof(TValue).Name, deliveryResult.Topic, deliveryResult.Partition, deliveryResult.Offset);
                
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            logger.LogError("Kafka produce error: {ErrorReason}", ex.Error.Reason);
            throw;
        }        
    }
}

