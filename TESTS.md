# Unit Tests - UserEventsApp

## Overview

A comprehensive unit test suite for the UserEventsApp project using **xUnit** and **Moq** frameworks.

## Test Projects

### 1. UserCreatedProducerTests
Located in `UserEvents.Tests/Kafka/UserCreatedProducerTests.cs`

**Tests:**
- ✅ `ProduceAsync_WithValidEvent_ShouldSucceed` - Validates successful message production
- ✅ `ProduceAsync_WithNullEvent_ShouldThrowArgumentNullException` - Null event validation
- ✅ `ProduceAsync_WhenKafkaThrowsException_ShouldRethrow` - Error handling
- ✅ `ProduceAsync_ShouldUseConfiguredTopic` - Configuration usage validation

**Purpose:** Ensures the producer correctly serializes and sends events to Kafka with proper error handling.

### 2. UserCreationHandlerTests
Located in `UserEvents.Tests/Kafka/UserCreationHandlerTests.cs`

**Tests:**
- ✅ `HandleAsync_WithValidEvent_ShouldComplete` - Valid event processing
- ✅ `HandleAsync_WithNullEvent_ShouldThrowArgumentNullException` - Null validation
- ✅ `HandleAsync_WithMultipleEvents_ShouldHandleAllSuccessfully` - Batch processing
- ✅ `HandleAsync_WithValidUserData_ShouldPreserveAllFields` - Data integrity

**Purpose:** Tests the event handler processes user creation events correctly and preserves data.

### 3. UserCreatedEventTests
Located in `UserEvents.Tests/Models/UserCreatedEventTests.cs`

**Tests:**
- ✅ `UserCreatedEvent_WithAllProperties_ShouldCreateSuccessfully` - Object creation
- ✅ `UserCreatedEvent_ImplementsISpecificRecord` - Interface compliance
- ✅ `UserCreatedEvent_Get_WithValidFieldPosition_ShouldReturnCorrectValue` - Avro Get method
- ✅ `UserCreatedEvent_Get_WithInvalidFieldPosition_ShouldThrowException` - Error handling
- ✅ `UserCreatedEvent_Put_WithValidFieldPosition_ShouldSetCorrectValue` - Avro Put method
- ✅ `UserCreatedEvent_Put_WithInvalidFieldPosition_ShouldThrowException` - Error handling
- ✅ `UserCreatedEvent_EmailValidation_ShouldAcceptValidEmails` - Email validation
- ✅ `UserCreatedEvent_CreatedAt_ShouldStoreUnixTimestamp` - Timestamp handling
- ✅ `UserCreatedEvent_PropertiesAreModifiable` - Property mutation

**Purpose:** Validates the Avro ISpecificRecord implementation and data model integrity.

### 4. KafkaConfigTests
Located in `UserEvents.Tests/Config/KafkaConfigTests.cs`

**Tests:**
- ✅ `KafkaConfig_WithValidSettings_ShouldCreateSuccessfully` - Configuration creation
- ✅ `KafkaConfig_DefaultValues_ShouldBeSet` - Default value verification
- ✅ `KafkaConfig_WithMultipleBootstrapServers_ShouldBeSupported` - Multi-broker support
- ✅ `KafkaConfig_CanBeConfiguredViaOptions` - Options pattern compatibility
- ✅ `UserEventsConfig_WithKafkaConfig_ShouldCreateSuccessfully` - Nested configuration
- ✅ `KafkaConfig_SchemaRegistryUrl_ShouldSupportHttpsProtocol` - Protocol support

**Purpose:** Ensures configuration loading works correctly with various settings and defaults.

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test UserEvents.Tests/UserEvents.Tests.csproj
```

### Run Tests with Verbose Output
```bash
dotnet test --verbosity normal
```

### Run Specific Test Class
```bash
dotnet test --filter "UserCreatedProducerTests"
```

### Run Specific Test Method
```bash
dotnet test --filter "UserCreatedProducerTests.ProduceAsync_WithValidEvent_ShouldSucceed"
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Test Coverage

| Component | Tests | Status |
|-----------|-------|--------|
| Producer | 4 | ✅ Complete |
| Event Handler | 4 | ✅ Complete |
| Data Model | 9 | ✅ Complete |
| Configuration | 6 | ✅ Complete |
| **Total** | **23** | **✅ Complete** |

## Technologies Used

- **xUnit** v2.6.6 - Testing framework
- **Moq** v4.20.70 - Mocking framework
- **Microsoft.NET.Test.Sdk** v17.9.0 - Test SDK

## Test Conventions

### Naming Convention
- `MethodName_Scenario_ExpectedBehavior`
- Example: `ProduceAsync_WithValidEvent_ShouldSucceed`

### Arrange-Act-Assert Pattern
All tests follow the AAA pattern:
```csharp
// Arrange - Setup test data and mocks
var event = new UserCreatedEvent { /* ... */ };

// Act - Perform the action being tested
await producer.ProduceAsync("key", event);

// Assert - Verify the results
Assert.True(mockProducer.Verify(...));
```

### Using Mocks
Tests use Moq to isolate components:
```csharp
var mockProducer = new Mock<IProducer<string, UserCreatedEvent>>();
mockProducer
    .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, UserCreatedEvent>>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(deliveryReport);
```

## Contributing Tests

When adding new features, please include:
1. ✅ Unit tests for the new functionality
2. ✅ Edge case tests (null values, empty collections, etc.)
3. ✅ Error handling tests
4. ✅ Integration points with existing components

Guidelines:
- Keep tests focused - one behavior per test
- Use meaningful test names
- Mock external dependencies
- Maintain >85% code coverage

## Future Enhancements

- [ ] Integration tests with Docker-based Kafka
- [ ] Performance benchmarking tests
- [ ] Load testing scenarios
- [ ] CI/CD pipeline integration
- [ ] Coverage reporting

## References

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
