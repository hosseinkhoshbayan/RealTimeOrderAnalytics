# 🛠️ Development Guide

Complete guide for developing and extending the Order API.

## 📋 Table of Contents

- [Project Structure](#project-structure)
- [Architecture Overview](#architecture-overview)
- [Services Layer](#services-layer)
- [Validators Layer](#validators-layer)
- [Adding New Features](#adding-new-features)
- [Testing](#testing)
- [Best Practices](#best-practices)

---

## 📁 Project Structure

```
OrderApi/
├── Models/                          # Data models and DTOs
│   └── Order.cs                     # Order model and response types
├── Services/                        # Business services
│   ├── IMessagePublisher.cs         # Message publisher interface
│   ├── RabbitMqPublisher.cs         # RabbitMQ implementation
│   ├── OrderService.cs              # Order business logic
│   └── HealthCheckService.cs        # Health check service
├── Validators/                      # Input validation
│   └── OrderValidator.cs            # Order validation rules
├── Program.cs                       # Application entry point
├── appsettings.json                # Configuration
└── OrderApi.csproj                 # Project file

OrderApi.Tests/
├── OrderValidatorTests.cs          # Validator unit tests
└── OrderApi.Tests.csproj           # Test project file
```

---

## 🏗️ Architecture Overview

### Three-Layer Architecture

```
┌─────────────────────────────────────┐
│     Presentation Layer (API)        │  ← Program.cs (Endpoints)
├─────────────────────────────────────┤
│     Business Logic Layer            │  ← Services + Validators
├─────────────────────────────────────┤
│     Infrastructure Layer            │  ← RabbitMQ, External Services
└─────────────────────────────────────┘
```

### Request Flow

```
1. HTTP Request → API Endpoint (Program.cs)
                    ↓
2. Validator → OrderValidator.Validate()
                    ↓
3. Service → OrderService.CreateOrderAsync()
                    ↓
4. Publisher → RabbitMqPublisher.PublishOrderAsync()
                    ↓
5. RabbitMQ → Message Queue
                    ↓
6. HTTP Response → Client
```

---

## 🔧 Services Layer

### IMessagePublisher Interface

**Purpose**: Abstracts message publishing to allow different implementations

```csharp
public interface IMessagePublisher
{
    Task PublishOrderAsync(Order order);
    bool IsConnected();
    ConnectionStatus GetConnectionStatus();
}
```

**Why?**
- Dependency Inversion Principle
- Easy to mock for testing
- Can switch implementations (Kafka, Azure Service Bus, etc.)

### RabbitMqPublisher Implementation

**Features**:
- ✅ Connection management
- ✅ Message serialization
- ✅ Error handling and logging
- ✅ Proper resource disposal

**Key Methods**:

```csharp
// Publish a message
await _messagePublisher.PublishOrderAsync(order);

// Check connection
bool isConnected = _messagePublisher.IsConnected();

// Get detailed status
var status = _messagePublisher.GetConnectionStatus();
```

### OrderService

**Purpose**: Orchestrates order creation workflow

**Responsibilities**:
1. Validate order
2. Check broker connection
3. Publish order
4. Track statistics
5. Handle errors

**Usage**:

```csharp
var response = await _orderService.CreateOrderAsync(order);

if (response.Success)
{
    // Order was queued successfully
}
```

### HealthCheckService

**Purpose**: Monitors application health

**Usage**:

```csharp
var health = _healthCheckService.GetHealthStatus();
// Returns: { status: "healthy", rabbitMqConnected: true, ... }
```

---

## ✅ Validators Layer

### OrderValidator

**Purpose**: Validates orders against business rules

**Validation Rules**:

| Field | Rules |
|-------|-------|
| OrderId | Required, Max 50 chars, Format: `XXX-123456` |
| ProductId | Required, Max 50 chars, Format: `PROD-XXXXX` |
| Quantity | Required, Between 1 and 1000 |
| CreatedAt | Cannot be in future |

**Usage**:

```csharp
// Full validation
var result = OrderValidator.Validate(order);
if (!result.IsValid)
{
    Console.WriteLine(result.ErrorMessage);
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Quick validation
var (isValid, errorMessage) = OrderValidator.ValidateQuick(order);

// Critical fields only
bool isValid = OrderValidator.ValidateCriticalFields(order);

// Get all rules
var rules = OrderValidator.GetValidationRules();
```

**Extending Validation**:

```csharp
// Add custom validation
private static List<string> ValidateCustomField(string field)
{
    var errors = new List<string>();
    
    if (string.IsNullOrEmpty(field))
    {
        errors.Add("Custom field is required");
    }
    
    return errors;
}

// Update Validate method
public static ValidationResult Validate(Order order)
{
    var errors = new List<string>();
    
    errors.AddRange(ValidateOrderId(order.OrderId));
    errors.AddRange(ValidateProductId(order.ProductId));
    errors.AddRange(ValidateQuantity(order.Quantity));
    errors.AddRange(ValidateCustomField(order.CustomField)); // New
    
    return errors.Count == 0 
        ? ValidationResult.Success() 
        : ValidationResult.Failure(errors);
}
```

---

## 🆕 Adding New Features

### Adding a New Service

**1. Create Interface:**

```csharp
public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
}
```

**2. Create Implementation:**

```csharp
public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    
    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementation
        _logger.LogInformation("📧 Email sent to {To}", to);
    }
}
```

**3. Register in Program.cs:**

```csharp
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
```

**4. Use in Endpoint:**

```csharp
app.MapPost("/api/notify", async (
    string email,
    INotificationService notificationService) =>
{
    await notificationService.SendEmailAsync(
        email, 
        "Order Confirmation", 
        "Your order has been received");
    
    return Results.Ok();
});
```

### Adding a New Endpoint

```csharp
app.MapGet("/api/orders/{id}", (
    string id,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Fetching order: {Id}", id);
    
    // Your logic here
    
    return Results.Ok(new { orderId = id });
})
.WithName("GetOrderById")
.WithTags("Orders")
.WithSummary("Get order by ID")
.Produces(200)
.Produces(404);
```

### Adding a New Validator

```csharp
public static class ProductValidator
{
    public static ValidationResult Validate(Product product)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            errors.Add("Product name is required");
        }
        
        if (product.Price <= 0)
        {
            errors.Add("Price must be greater than 0");
        }
        
        return errors.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure(errors);
    }
}
```

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~OrderValidatorTests.Validate_ValidOrder_ReturnsSuccess"
```

### Writing Unit Tests

**Example - Testing a Validator:**

```csharp
[Fact]
public void Validate_ValidOrder_ReturnsSuccess()
{
    // Arrange
    var order = new Order("ORD-001", "PROD-123", 5);

    // Act
    var result = OrderValidator.Validate(order);

    // Assert
    Assert.True(result.IsValid);
    Assert.Null(result.ErrorMessage);
}
```

**Example - Testing a Service (with Mocking):**

```csharp
[Fact]
public async Task CreateOrderAsync_ValidOrder_ReturnsSuccess()
{
    // Arrange
    var mockPublisher = new Mock<IMessagePublisher>();
    mockPublisher.Setup(p => p.IsConnected()).Returns(true);
    mockPublisher.Setup(p => p.PublishOrderAsync(It.IsAny<Order>()))
                 .Returns(Task.CompletedTask);
    
    var mockLogger = new Mock<ILogger<OrderService>>();
    var service = new OrderService(mockPublisher.Object, mockLogger.Object);
    
    var order = new Order("ORD-001", "PROD-123", 5);

    // Act
    var result = await service.CreateOrderAsync(order);

    // Assert
    Assert.True(result.Success);
    mockPublisher.Verify(p => p.PublishOrderAsync(order), Times.Once);
}
```

---

## 📚 Best Practices

### 1. Dependency Injection

**✅ Do:**
```csharp
public class OrderService
{
    private readonly IMessagePublisher _publisher;
    
    public OrderService(IMessagePublisher publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }
}
```

**❌ Don't:**
```csharp
public class OrderService
{
    private RabbitMqPublisher _publisher = new RabbitMqPublisher(); // Tight coupling
}
```

### 2. Logging

**✅ Do:**
```csharp
_logger.LogInformation("Order created: {OrderId}", order.OrderId);
_logger.LogError(ex, "Failed to process order: {OrderId}", order.OrderId);
```

**❌ Don't:**
```csharp
Console.WriteLine("Order created: " + order.OrderId); // Not structured
_logger.LogInformation($"Order created: {order.OrderId}"); // String interpolation
```

### 3. Error Handling

**✅ Do:**
```csharp
try
{
    await _publisher.PublishOrderAsync(order);
    return new OrderResponse { Success = true };
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to publish order");
    return new OrderResponse 
    { 
        Success = false, 
        Message = "Service temporarily unavailable" 
    };
}
```

**❌ Don't:**
```csharp
try
{
    await _publisher.PublishOrderAsync(order);
}
catch (Exception ex)
{
    throw; // Swallow or rethrow without logging
}
```

### 4. Async/Await

**✅ Do:**
```csharp
public async Task<OrderResponse> CreateOrderAsync(Order order)
{
    await _publisher.PublishOrderAsync(order);
    return new OrderResponse { Success = true };
}
```

**❌ Don't:**
```csharp
public OrderResponse CreateOrder(Order order)
{
    _publisher.PublishOrderAsync(order).Wait(); // Blocking
    return new OrderResponse { Success = true };
}
```

### 5. Resource Disposal

**✅ Do:**
```csharp
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    public void Dispose()
    {
        if (_connection?.IsOpen == true)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
```

---

## 🔗 Related Documentation

- [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - API endpoints reference
- [README.md](README.md) - Project overview
- [CHANGELOG.md](CHANGELOG.md) - Version history

---

## 💡 Tips

1. **Always validate input** before processing
2. **Use structured logging** with context
3. **Handle errors gracefully** and return meaningful messages
4. **Write tests** for critical business logic
5. **Follow SOLID principles** for maintainable code
6. **Document your code** with XML comments
7. **Use async/await** for I/O operations

---

**Happy Coding! 🚀**