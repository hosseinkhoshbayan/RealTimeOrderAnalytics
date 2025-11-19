using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OrderApi;
using OrderApi.Models;
using OrderApi.Services;
using OrderApi.Validators;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURE SERVICES =====

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order API",
        Version = "v2.0",
        Description = "RESTful API for managing orders in a microservices architecture",
        Contact = new OpenApiContact
        {
            Name = "Order API Team",
            Url = new Uri("https://github.com/yourusername/RealTimeOrderAnalytics")
        }
    });
});

// Configure RabbitMQ Connection
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory { HostName = rabbitMqHost };
    var logger = sp.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("🔌 Connecting to RabbitMQ at {Host}...", rabbitMqHost);
        var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        logger.LogInformation("✅ Connected to RabbitMQ successfully");
        return connection;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Failed to connect to RabbitMQ at {Host}", rabbitMqHost);
        throw;
    }
});

// Register Services
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// ===== CONFIGURE MIDDLEWARE =====

app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v2.0");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// ===== API ENDPOINTS =====

// Root endpoint
app.MapGet("/", () => new
{
    service = "Order API",
    version = "2.0.0",
    status = "running",
    documentation = "/swagger",
    endpoints = new[]
    {
        "GET / - API information",
        "GET /health - Health check",
        "GET /api/validation-rules - Get validation rules",
        "POST /api/orders - Create new order",
        "GET /api/orders/stats - Get order statistics"
    }
})
.WithName("GetApiInfo")
.WithTags("Info")
.WithSummary("Get API information")
.WithDescription("Returns basic information about the API service");

// Health Check
app.MapGet("/health", (IHealthCheckService healthService) =>
{
    var health = healthService.GetHealthStatus();

    return health.Status == "healthy"
        ? Results.Ok(health)
        : Results.StatusCode(503);
})
.WithName("HealthCheck")
.WithTags("Health")
.WithSummary("Check API health status")
.Produces<HealthResponse>(200)
.Produces(503);

// Get Validation Rules
app.MapGet("/api/validation-rules", () =>
{
    var rules = OrderApi.Validators.OrderValidator.GetValidationRules();
    return Results.Ok(new { rules });
})
.WithName("GetValidationRules")
.WithTags("Info")
.WithSummary("Get order validation rules")
.Produces(200);

// Create Order (RESTful)
app.MapPost("/api/orders", async (
    [FromBody] Order order,
    IOrderService orderService,
    ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("📨 Received order request: {OrderId}", order.OrderId);

        var response = await orderService.CreateOrderAsync(order);

        if (!response.Success)
        {
            logger.LogWarning("⚠️ Order rejected: {Message}", response.Message);
            return Results.BadRequest(response);
        }

        logger.LogInformation("✅ Order accepted: {OrderId}", order.OrderId);
        return Results.Accepted($"/api/orders/{order.OrderId}", response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Unexpected error processing order");
        return Results.Problem(
            title: "Internal Server Error",
            detail: "An unexpected error occurred while processing your order",
            statusCode: 500
        );
    }
})
.WithName("CreateOrder")
.WithTags("Orders")
.WithSummary("Create a new order")
.WithDescription("Validates and queues a new order for processing")
.Produces<OrderResponse>(202)
.Produces<OrderResponse>(400)
.Produces(500);

// Legacy endpoint for backward compatibility
app.MapPost("/order", async (
    [FromBody] Order order,
    IOrderService orderService) =>
{
    var response = await orderService.CreateOrderAsync(order);

    if (!response.Success)
    {
        return Results.BadRequest(new { error = response.Message });
    }

    return Results.Accepted($"/order/{order.OrderId}", order);
})
.WithName("CreateOrderLegacy")
.WithTags("Orders")
.ExcludeFromDescription();

// Get Order Statistics
app.MapGet("/api/orders/stats", (IOrderService orderService) =>
{
    var stats = orderService.GetStatistics();
    return Results.Ok(stats);
})
.WithName("GetOrderStats")
.WithTags("Orders")
.WithSummary("Get order statistics")
.WithDescription("Returns statistics about processed orders")
.Produces<OrderStatsResponse>(200);

// Connection Status
app.MapGet("/api/connection-status", (IMessagePublisher publisher) =>
{
    var status = publisher.GetConnectionStatus();
    return Results.Ok(status);
})
.WithName("GetConnectionStatus")
.WithTags("Health")
.WithSummary("Get RabbitMQ connection status")
.Produces<ConnectionStatus>(200);

// ===== GRACEFUL SHUTDOWN =====

app.Lifetime.ApplicationStopping.Register(async () =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🛑 Shutting down Order API...");

    var connection = app.Services.GetRequiredService<IConnection>();
    if (connection.IsOpen)
    {
        await connection.CloseAsync();
        await connection.DisposeAsync();
        logger.LogInformation("✅ RabbitMQ connection closed");
    }
});

// ===== START APPLICATION =====

app.Logger.LogInformation("🚀 Order API v2.0 started successfully");
app.Logger.LogInformation("📍 Listening on: {Urls}", string.Join(", ", app.Urls));
app.Logger.LogInformation("📚 Swagger UI: {SwaggerUrl}",
    $"{app.Urls.FirstOrDefault()}/swagger");

app.Run();

