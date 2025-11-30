using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using UserEvents.Infra;
using UserEvents.Models.Config;
using Confluent.SchemaRegistry;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Confluent.SchemaRegistry.Serdes;
using UserEvents.Infra.Interfaces;
using UserEvents.Infra.Kafka;
using Confluent.Kafka.SyncOverAsync;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            config.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
        })
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext();
        })
        .ConfigureServices((hostingContext, services) =>
        {
            services.AddOptions<UserEventsConfig>();
            services.Configure<UserEventsConfig>(hostingContext.Configuration);

            // Register Schema Registry Client
            services.AddSingleton<ISchemaRegistryClient>(provider =>
            {
                var config = provider.GetRequiredService<IOptions<UserEventsConfig>>().Value;
                return new CachedSchemaRegistryClient(new SchemaRegistryConfig
                {
                    Url = config.KafkaConfig.SchemaRegistryUrl
                });
            });

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IOptions<UserEventsConfig>>().Value;
                var schemaRegistryClient = provider.GetRequiredService<ISchemaRegistryClient>();

                var producerConfig = new ProducerConfig
                {
                    BootstrapServers = config.KafkaConfig.BootstrapServers
                };

                return new ProducerBuilder<string, UserCreatedEvent>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<UserCreatedEvent>(schemaRegistryClient))
                    .Build();
            });

            services.AddSingleton<
                IEventProducer<string, UserCreatedEvent>,
                UserCreatedProducer<string, UserCreatedEvent>>();

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IOptions<UserEventsConfig>>().Value;
                var schemaRegistryClient = provider.GetRequiredService<ISchemaRegistryClient>();

                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = config.KafkaConfig.BootstrapServers,
                    GroupId = config.KafkaConfig.ConsumerGroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };

                return new ConsumerBuilder<string, UserCreatedEvent>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<UserCreatedEvent>(schemaRegistryClient).AsSyncOverAsync())
                    .Build();
            });

            services.AddTransient<IEventHandler<UserCreatedEvent>, UserCreationHandler>();
            services.AddHostedService<UserEventsConsumerService>();
        });

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
