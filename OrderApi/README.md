# 📦 Order API (C# / ASP.NET Core)

RESTful API service for creating and managing orders, built with ASP.NET Core 8.0.

## 🏗️ Architecture

```
Controllers/Endpoints
        ↓
   Validators
        ↓
    Services
        ↓
   RabbitMQ Publisher
```

## 📁 Project Structure

```
OrderApi/
├── Models/
│   └── Order.cs                 # Data models
├── Services/
│   ├── IMessagePublisher.cs     # Publisher interface
│   └── RabbitMqPublisher.cs     # RabbitMQ implementation
├── Validators/
│   └── OrderValidator.cs        # Business logic validation
├── Program.cs                   # Application entry point
├── appsettings.json            # Configuration
├── OrderApi.csproj             # Project file
└── Dockerfile                  # Docker configuration
```

## 🚀 Running Locally

### Prerequisites
- .NET 8.0 SDK
- RabbitMQ (or Docker)

### Steps

1. **Start RabbitMQ:**
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

2. **Restore dependencies:**
```bash
dotnet restore
```

3. **Run the application:**
```bash
dotnet run
```

4. **Access the API:**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger

## 🐳 Running with Docker

```bash
docker build -t order-api .
docker run -p 8080:8080 -e RABBITMQ_HOST=host.docker.internal order-api
```

## 📡 API Endpoints

### Health Check
```bash
curl http://localhost:8080/health
```

### Create Order
```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-001",
    "ProductId": "PROD-123",
    "Quantity": 5
  }'
```

### Get Statistics
```bash
curl http://localhost:8080/api/orders/stats
```

## ✅ Validation Rules

Orders are validated before being published:

- **OrderId**: Required, non-empty
- **ProductId**: Required, non-empty  
- **Quantity**: Must be between 1 and 1000

Example validation error:
```json
{
  "success": false,
  "message": "Quantity must be greater than 0"
}
```

## 🔧 Configuration

Configure via environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `RABBITMQ_HOST` | RabbitMQ server hostname | `localhost` |
| `ASPNETCORE_URLS` | URLs to listen on | `http://+:8080` |

## 🧪 Testing

### Unit Tests (to be implemented)
```bash
dotnet test
```

### Integration Testing
Use the included test script:
```bash
../test-system.sh
```

## 📦 Dependencies

- **RabbitMQ.Client** (6.8.1) - AMQP client for RabbitMQ
- **Swashbuckle.AspNetCore** (6.5.0) - Swagger/OpenAPI tooling

## 🏛️ Design Patterns

### Dependency Injection
Services are registered in `Program.cs`:
```csharp
builder.Services.AddSingleton<IConnection>(...);
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
```

### Repository Pattern
`IMessagePublisher` abstracts the message queue implementation

### Validator Pattern
`OrderValidator` separates validation logic from business logic

## 🔒 CORS Configuration

CORS is enabled for all origins (development only):
```csharp
app.UseCors("AllowAll");
```

**⚠️ Production**: Configure specific origins

## 📊 Logging

Structured logging with different levels:
```csharp
logger.LogInformation("📦 Order created: {OrderId}", order.OrderId);
logger.LogError(ex, "❌ Error creating order");
```

## 🛠️ Extending the API

### Add a new endpoint:
```csharp
app.MapGet("/api/orders/{id}", (string id) => {
    // Your logic here
})
.WithName("GetOrder")
.WithTags("Orders");
```

### Add a new service:
```csharp
// 1. Create interface
public interface IMyService { }

// 2. Create implementation
public class MyService : IMyService { }

// 3. Register in Program.cs
builder.Services.AddSingleton<IMyService, MyService>();
```

## 🐛 Troubleshooting

### RabbitMQ Connection Failed
```
Failed to connect to RabbitMQ at localhost
```
**Solution**: Ensure RabbitMQ is running on the correct host

### Port Already in Use
```
Unable to bind to http://localhost:8080
```
**Solution**: Change port via environment variable:
```bash
export ASPNETCORE_URLS=http://+:8081
dotnet run
```

## 📚 Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [Minimal APIs](https://docs.microsoft.com/aspnet/core/fundamentals/minimal-apis)

## 🎯 Future Enhancements

- [ ] Add authentication/authorization
- [ ] Implement rate limiting
- [ ] Add comprehensive unit tests
- [ ] Add integration tests
- [ ] Implement order status tracking
- [ ] Add database for order persistence