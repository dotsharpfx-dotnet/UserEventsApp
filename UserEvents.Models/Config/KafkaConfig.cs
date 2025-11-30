namespace UserEvents.Models.Config;

public class KafkaConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ConsumerGroupId { get; set; }
    public string SchemaRegistryUrl { get; set; } = "http://localhost:8081";
    public string Topic { get; set; } = "user-events";
}
