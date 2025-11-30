# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["UserEventsApp.sln", "."]
COPY ["UserEvents.App/UserEvents.App.csproj", "UserEvents.App/"]
COPY ["UserEvents.Infra/UserEvents.Infra.csproj", "UserEvents.Infra/"]
COPY ["UserEvents.Models/UserEvents.Models.csproj", "UserEvents.Models/"]

# Restore dependencies
RUN dotnet restore "UserEventsApp.sln"

# Copy source code
COPY . .

# Build application
RUN dotnet build "UserEventsApp.sln" -c Release --no-restore

# Publish
RUN dotnet publish "UserEvents.App/UserEvents.App.csproj" -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Install necessary tools
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_TieredCompilation=1
ENV DOTNET_ReadyToRun=1

# Expose port
EXPOSE 8080

# Run application
ENTRYPOINT ["dotnet", "UserEvents.App.dll"]
