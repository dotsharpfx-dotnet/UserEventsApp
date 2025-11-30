# Integrating Kafka Messaging Using Avro Schema in a .NET Hosted Service: A Practical Guide

Building scalable event-driven systems in .NET doesn't have to be complicated. But managing schema evolution across distributed services? That's where most teams struggle. This guide focuses on how Avro solves this problem when used with Apache Kafka and .NET 10.

## Why Avro With Kafka?

Kafka is excellent at moving messages around. But without a schema contract, chaos emerges quickly:

- Producer adds a new field. Consumers crash.
- Team A expects version 1 of the event. Team B sends version 2.
- Nobody knows what fields are actually required.

Avro fixes this. It's a schema-based serialization format that enforces structure and enables evolution. When combined with Confluent's Schema Registry, you get centralized schema management and automatic validation.

## Understanding the Avro Schema

Here's our schema for a user creation event:

```json
{
    "type": "record",
    "name": "UserCreatedEvent",
    "namespace": "App.Events",
    "fields": [
        { "name": "UserId", "type": "string" },
        { "name": "Username", "type": "string" },
        { "name": "UserEmail", "type": "string" },
        { "name": "CreatedAt", "type": "long" }
    ]
}
```

This schema defines what a user creation event looks like. Each field has a name and a type. Avro is strongly typed—no surprises at runtime.

## The C# Model: ISpecificRecord

To serialize and deserialize with Avro in .NET, your model must implement `ISpecificRecord`:

```csharp
using Avro;
using Avro.Specific;

public class UserCreatedEvent : ISpecificRecord
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string UserEmail { get; set; }
    public long CreatedAt { get; set; }

    public Schema Schema { get; set; }

    public object Get(int fieldPos) =>
        fieldPos switch
        {
            0 => UserId,
            1 => Username,
            2 => UserEmail,
            3 => CreatedAt,
            _ => throw new AvroRuntimeException($"Bad index {fieldPos}")
        };

    public void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            case 0:
                UserId = (string)fieldValue;
                break;
            case 1:
                Username = (string)fieldValue;
                break;
            case 2:
                UserEmail = (string)fieldValue;
                break;
            case 3:
                CreatedAt = (long)fieldValue;
                break;
        }
    }
}
```

Why `ISpecificRecord`? The Avro serializer works with this interface to map between your C# properties and Avro's field representation. The `Get` and `Put` methods act as the bridge—they're called during serialization and deserialization.

## Setting Up Infrastructure

Schema Registry must run alongside Kafka. Use Docker Compose:

```yaml
schema-registry:
  image: confluentinc/cp-schema-registry:7.5.0
  environment:
    SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS: kafka:29092
    SCHEMA_REGISTRY_LISTENERS: http://0.0.0.0:8081
  ports:
    - "8081:8081"
```

Schema Registry stores all versions of your schemas. When a producer sends a message, the serializer registers the schema and includes its ID in the Kafka message. Consumers use that ID to fetch and validate the schema.

## Producing With Avro Serialization

The key is using `AvroSerializer`:

```csharp
var producerConfig = new ProducerConfig
{
    BootstrapServers = "localhost:9092"
};

var producer = new ProducerBuilder<string, UserCreatedEvent>(producerConfig)
    .SetValueSerializer(
        new AvroSerializer<UserCreatedEvent>(schemaRegistryClient)
            .AsSyncOverAsync())
    .Build();

var userEvent = new UserCreatedEvent
{
    UserId = "user-123",
    Username = "john_doe",
    UserEmail = "john@example.com",
    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
};

await producer.ProduceAsync("user-events", 
    new Message<string, UserCreatedEvent> { Key = "user-123", Value = userEvent });
```

When you call `ProduceAsync`:
1. The `AvroSerializer` validates against the schema
2. It serializes the event to binary Avro format
3. The schema ID is embedded in the message
4. Kafka receives the message

If the event doesn't match the schema, you get an exception immediately. Schema validation happens at produce time, not consume time.

## Consuming With Avro Deserialization

On the consumer side:

```csharp
var consumerConfig = new ConsumerConfig
{
    BootstrapServers = "localhost:9092",
    GroupId = "user-events-consumer-group",
    AutoOffsetReset = AutoOffsetReset.Earliest
};

var consumer = new ConsumerBuilder<string, UserCreatedEvent>(consumerConfig)
    .SetValueDeserializer(
        new AvroDeserializer<UserCreatedEvent>(schemaRegistryClient)
            .AsSyncOverAsync())
    .Build();

consumer.Subscribe("user-events");

while (true)
{
    var result = consumer.Consume();
    var userEvent = result.Message.Value;
    
    Console.WriteLine($"User: {userEvent.UserId}, Email: {userEvent.UserEmail}");
    consumer.Commit(result);
}
```

The `AvroDeserializer`:
1. Reads the schema ID from the message
2. Fetches that schema from Registry
3. Deserializes the binary data into your C# object
4. Returns the strongly-typed event

If the schema has evolved, the deserializer handles it automatically.

## Schema Evolution: The Real Value

This is where Avro shines. Six months later, you need to track when users confirmed their email:

```json
{
    "type": "record",
    "name": "UserCreatedEvent",
    "namespace": "App.Events",
    "fields": [
        { "name": "UserId", "type": "string" },
        { "name": "Username", "type": "string" },
        { "name": "UserEmail", "type": "string" },
        { "name": "CreatedAt", "type": "long" },
        { "name": "EmailConfirmedAt", "type": ["null", "long"], "default": null }
    ]
}
```

Notice the new field is `["null", "long"]` with a default of `null`. This is backward compatible.

Now:
- **Old producers** that don't know about `EmailConfirmedAt` still work. Schema Registry accepts them.
- **New consumers** get `null` for `EmailConfirmedAt` on old events.
- **New producers** populate `EmailConfirmedAt` for new events.

Update your C# model:

```csharp
public class UserCreatedEvent : ISpecificRecord
{
    // ... existing fields ...
    public long? EmailConfirmedAt { get; set; }

    public object Get(int fieldPos) =>
        fieldPos switch
        {
            // ... existing cases ...
            4 => EmailConfirmedAt,
            _ => throw new AvroRuntimeException($"Bad index {fieldPos}")
        };

    public void Put(int fieldPos, object fieldValue)
    {
        switch (fieldPos)
        {
            // ... existing cases ...
            case 4:
                EmailConfirmedAt = (long?)fieldValue;
                break;
        }
    }
}
```

No downtime. No coordination across teams. This is the power of schema evolution.

## Why ISpecificRecord Matters

You might wonder: "Why not use a simpler POCO?" Because Avro needs runtime reflection into your object structure. `ISpecificRecord` provides that directly through `Get` and `Put`. Without it, Avro has to use reflection, which is slower and more fragile.

The boilerplate is worth it for performance and reliability.

## Configuration Best Practices

Keep Kafka settings external:

```json
{
    "KafkaConfig": {
        "BootstrapServers": "localhost:9092",
        "Topic": "user-events",
        "ConsumerGroupId": "user-events-consumer",
        "SchemaRegistryUrl": "http://localhost:8081"
    }
}
```

This way, your production deployment points to production Kafka without code changes. Same for Schema Registry.

## Common Pitfalls

**Not Registering Schema Registry Client as Singleton**: Create it once, reuse it. Schema Registry is thread-safe and caches schemas locally.

**Ignoring Field Ordering in ISpecificRecord**: The field positions in `Get` and `Put` must match your schema definition exactly. Off by one and deserialization fails silently.

**Forgetting Default Values**: When evolving schemas, new fields need defaults. Otherwise, old messages can't be deserialized by consumers that expect the new field.

**Not Committing Offsets**: If your consumer crashes, it needs to resume from where it left off. Always commit after successful processing.

## The Flow in Practice

1. **Define** your schema in JSON
2. **Register** it with Schema Registry (automatic on first produce)
3. **Generate** or write your C# `ISpecificRecord` class
4. **Produce** messages with `AvroSerializer`
5. **Consume** messages with `AvroDeserializer`
6. **Evolve** by updating both schema and model
7. **Repeat** without breaking existing services

## Getting Started

```bash
# Start infrastructure
docker compose up -d

# Build and run
dotnet build
dotnet run --project UserEvents.App/UserEvents.App.csproj
```

The complete working example is available in the repository. It includes producers, consumers, configuration, and Docker setup ready to go.

## Final Thoughts

Avro + Kafka + .NET 10 is a powerful combination for event-driven systems. Avro handles the complexity of schema management and evolution. Kafka provides the distribution. .NET gives you productivity and performance.

The investment in understanding `ISpecificRecord` and schema design pays dividends when you have dozens of services producing events. You avoid the "schema mismatch" bugs that plague microservices architectures.

Start with a simple event like we did here. Use Avro from day one. Your future self will thank you when it's time to add a new field without causing outages.

Happy building. 🚀

