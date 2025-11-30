# 👥 UserEventsApp

A .NET 10 application for consuming and producing user events via Apache Kafka with Avro serialization and Schema Registry.

## 📋 Prerequisites

### 💻 System Requirements
- 🏢 **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- 🐳 **Docker** - [Download](https://www.docker.com/products/docker-desktop)
- 🐳 **Docker Compose** - Included with Docker Desktop (or install separately)
- 🖥️ **macOS/Linux/Windows** with command-line terminal support

### ✅ Verify Installation
```bash
# Check .NET version
dotnet --version

# Check Docker version
docker --version

# Check Docker Compose version
docker compose version
```

## 📁 Project Structure

```
UserEventsApp/
├── UserEvents.App/           # Main console application
├── UserEvents.Infra/         # Infrastructure (Kafka producer/consumer)
├── UserEvents.Models/        # Data models and configuration
├── docker-compose.yml        # Docker services configuration
└── README.md                 # This file
```

### 🧩 Project Components

- 🚀 **UserEvents.App**: Entry point that runs the consumer service
- ⚙️ **UserEvents.Infra**: Contains Kafka producer/consumer implementations
- 📦 **UserEvents.Models**: Contains data models (UserCreatedEvent), configurations, and Avro schemas
- 🐳 **Docker Compose**: Manages Zookeeper, Kafka, and Schema Registry services

## 🚀 Getting Started

### 1️⃣ Start Docker Services

Start the required infrastructure services (Kafka, Zookeeper, Schema Registry):

```bash
docker compose up -d
```

This will:
- Start Zookeeper (port 2181)
- Start Kafka broker (ports 9092, 9101)
- Start Schema Registry (port 8081)
- Automatically create the `user-events` topic

Verify services are running:
```bash
docker compose ps
```

### 2️⃣ Build the Application

```bash
cd UserEventsApp
dotnet build
```

### 3️⃣ Run the Application

```bash
dotnet run --project UserEvents.App/UserEvents.App.csproj
```

The application will:
1. Load configuration from `appsettings.json`
2. Connect to Kafka broker at `localhost:9092`
3. Subscribe to the `user-events` topic
4. Listen for messages and process them

## ⚙️ Configuration

### 🔧 appsettings.json

Located in `UserEvents.App/appsettings.json`:

```json
{
    "Serilog": {
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning"
            }
        },
        "WriteTo": [
            {
                "Name": "Console"
            },
            {
                "Name": "File",
                "Args": {
                    "path": "Logs/log-.txt",
                    "rollingInterval": "Day"
                }
            }
        ]
    },
    "KafkaConfig": {
        "BootstrapServers": "localhost:9092",
        "Topic": "user-events",
        "ConsumerGroupId": "user-events-consumer-group",
        "SchemaRegistryUrl": "http://localhost:8081"
    }
}
```

**Configuration Options:**
- `BootstrapServers`: Kafka broker address
- `Topic`: Kafka topic name for user events
- `ConsumerGroupId`: Consumer group identifier
- `SchemaRegistryUrl`: Schema Registry endpoint for Avro schemas

## 📤 Producing UserCreatedEvent Messages

### 📨 Message Structure

```csharp
var userCreatedEvent = new UserCreatedEvent(
    UserId: "user-123",
    UserName: "john_doe",
    UserEmail: "john@example.com",
    CreatedAt: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
```

### 📤 Using the Producer

The `UserEventsConsumerService` has the `IEventProducer<string, UserCreatedEvent>` injected. Example:

```csharp
await producer.ProduceAsync("user-123", userCreatedEvent);
```

The producer will:
1. Serialize the event to Avro format
2. Register the schema with Schema Registry if needed
3. Publish the message to the `user-events` topic

## 🐳 Docker Services

### 📋 docker-compose.yml

The application uses four main services:

#### 🐘 Zookeeper
- **Image**: confluentinc/cp-zookeeper:7.5.0
- **Port**: 2181
- **Purpose**: Manages Kafka cluster coordination

#### 📨 Kafka
- **Image**: confluentinc/cp-kafka:7.5.0
- **Ports**: 9092 (external), 9101 (internal)
- **Features**: 
  - Auto-create topics enabled
  - 24-hour log retention
  - 1 partition, 1 replication factor

#### 📚 Schema Registry
- **Image**: confluentinc/cp-schema-registry:7.5.0
- **Port**: 8081
- **Purpose**: Manages Avro schemas for Kafka messages

#### ⚡ Kafka Init
- **Purpose**: Automatically creates the `user-events` topic on startup

### 💚 Health Checks

All services include health checks:
```bash
# View service health status
docker compose ps
```

### 🔧 Docker Commands

```bash
# Start services in the background
docker compose up -d

# View logs for all services
docker compose logs -f

# View logs for a specific service
docker compose logs -f kafka

# Stop all services
docker compose down

# Remove all data and containers
docker compose down -v

# Rebuild services
docker compose up -d --build
```

## 📝 Logging

The application uses **Serilog** for logging:

- 💻 **Console Output**: Real-time logs displayed in terminal
- 📄 **File Logs**: Stored in `Logs/log-YYYY-MM-DD.txt` with daily rotation

Log levels configured:
- Default: Information
- Microsoft: Warning
- System: Warning

View logs:
```bash
# Console output (when running the app)
dotnet run --project UserEvents.App/UserEvents.App.csproj

# File logs
tail -f Logs/log-*.txt
```

## 🐛 Debugging

### 💻 VS Code Debugging

Press `F5` or use the Debug panel to start debugging with breakpoints.

The launch configuration is pre-configured in `.vscode/launch.json`.

### 🔍 Troubleshooting

#### ⚠️ Topic Not Found Error
```
Confluent.Kafka.ConsumeException: Subscribed topic not available: user-events
```

**Solution**: 
1. Ensure Docker services are running: `docker compose ps`
2. Wait for Kafka to be healthy (status: "healthy")
3. The `kafka-init` service should create the topic automatically

#### ⚠️ Connection Refused
```
Error: Unable to connect to broker
```

**Solution**:
1. Start Docker services: `docker compose up -d`
2. Verify Kafka is running: `docker compose logs kafka`
3. Check bootstrap servers in `appsettings.json`

#### ⚠️ Schema Registry Errors
```
Error retrieving schema from registry
```

**Solution**:
1. Verify Schema Registry is running: `docker compose ps`
2. Check Schema Registry logs: `docker compose logs schema-registry`
3. Ensure `appsettings.json` has correct `SchemaRegistryUrl`

## 👨‍💻 Development

### 🔨 Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build UserEvents.App/UserEvents.App.csproj
```

### 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test UserEvents.App.Tests/
```

### 📚 Project Dependencies

- **Confluent.Kafka**: Kafka client library
- **Serilog**: Structured logging
- **Microsoft.Extensions.Hosting**: Host/dependency injection
- **Microsoft.Extensions.Configuration**: Configuration management
- **Avro**: Serialization format (via Confluent)

## 🏗️ Architecture

### 📥 Consumer Flow
1. Application starts and subscribes to `user-events` topic
2. Listens for `UserCreatedEvent` messages
3. Deserializes Avro messages using Schema Registry
4. Passes events to `UserCreationHandler` for processing
5. Commits offset after successful processing

### 📤 Producer Flow
1. Create `UserCreatedEvent` instance
2. Inject `IEventProducer<string, UserCreatedEvent>`
3. Call `ProduceAsync(key, event)`
4. Producer serializes to Avro format
5. Message published to Kafka topic

## ⚡ Performance Considerations

- **Consumer Group**: `user-events-consumer-group` allows parallel consumers
- **Partitions**: Currently 1 partition (scalable for production)
- **Replication Factor**: 1 (recommended: 3 for production)
- **Auto-commit**: Disabled for safer offset management

## 🚀 Production Deployment

For production deployment:

1. **Increase replication factor** in docker-compose.yml
2. **Configure multiple partitions** for parallelization
3. **Update bootstrap servers** to point to production Kafka cluster
4. **Set Schema Registry URL** to production registry
5. **Configure proper logging levels** (reduce to Warning/Error)
6. **Use environment variables** for sensitive configuration
7. **Implement circuit breakers** for fault tolerance
8. **Monitor metrics** with Application Insights or similar

## 📚 Support & Documentation

- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent Kafka .NET Client](https://github.com/confluentinc/confluent-kafka-dotnet)
- [Avro Specification](https://avro.apache.org/docs/)
- [Serilog Documentation](https://serilog.net/)

## 🚧 Upcoming Changes

We have planned the following improvements and features for future releases:

### Testing Infrastructure
- 🧪 **Unit Tests** - Comprehensive unit test coverage for core services and business logic
- 🔧 **Functional Tests** - End-to-end functional testing for Kafka producer/consumer flows
- 📊 **Integration Tests** - Docker-based integration tests with live Kafka services
- 📈 **Code Coverage** - Target 80%+ code coverage across all projects

### How to Contribute to Testing
If you'd like to help with testing implementation, we welcome contributions! Please:
1. Create an issue with the label `enhancement: testing`
2. Discuss your approach before starting
3. Submit a PR with your test implementations

Stay tuned for updates! 📝

## 📄 License

This project is proprietary and confidential. All rights reserved.

**Restrictions:**
- ❌ Commercial use prohibited without permission
- ❌ Modification prohibited
- ❌ Distribution prohibited
- ❌ Reverse engineering prohibited
- ⚠️ Unauthorized access strictly forbidden

For licensing inquiries, please contact the copyright holder.

## 🤝 Contributing

We welcome contributions from developers! Here's how you can contribute to this project:

### Getting Started
1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/UserEventsApp.git`
3. Create a feature branch: `git checkout -b feature/your-feature-name`
4. Make your changes and commit: `git commit -m "Add your feature description"`
5. Push to your branch: `git push origin feature/your-feature-name`
6. Open a Pull Request with a clear description of your changes

### Contribution Guidelines

**Code Standards:**
- Follow C# naming conventions and .NET best practices
- Write clean, readable code with meaningful comments
- Ensure your code compiles without warnings

**Pull Request Process:**
1. Update the README.md if you change functionality
2. Verify the application builds: `dotnet build`
3. Provide a clear description of your changes
4. Link any related issues in the PR description

**Types of Contributions Welcome:**
- 🐛 Bug fixes and issue reports
- ✨ New features and enhancements
- 📚 Documentation improvements
- 💡 Performance optimizations
- 🎨 UI/UX improvements

### Reporting Issues
- Use the issue tracker to report bugs
- Provide clear reproduction steps
- Include error messages and logs
- Specify your environment (OS, .NET version, Docker version)

### Development Setup
```bash
# Install dependencies
dotnet restore

# Build the project
dotnet build

# Start Docker services
docker compose up -d
```

### Communication
- Ask questions in issues or discussions
- Follow the Code of Conduct (be respectful and professional)
- Be patient and constructive with feedback

### Questions?
If you have questions about contributing, please open an issue with the label `question` or reach out to the maintainers.

Thank you for helping make this project better! 🙌
