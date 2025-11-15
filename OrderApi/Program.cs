using System.Text;
using RabbitMQ.Client;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ---- تنظیمات اتصال RabbitMQ ----
// ---- RabbitMQ Connection Settings ----
var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
var factory = new ConnectionFactory { HostName = rabbitMqHost };

// ۱. اتصال (Connection) را یک بار در سطح اپلیکیشن بسازید
IConnection connection = await factory.CreateConnectionAsync();

// ۲. صف (Queue) را یک بار در زمان راه‌اندازی تعریف کنید
var setupChannel = await connection.CreateChannelAsync();
await setupChannel.QueueDeclareAsync(
    queue: "order_placed",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

// ---- مدیریت خاموش شدن برنامه ----
app.Lifetime.ApplicationStopping.Register(async () =>
{
    await connection.CloseAsync();
    await connection.DisposeAsync();
});

// ---- API Endpoint ----
app.MapPost("/order", async (Order order) =>
{
    // ایجاد کانال برای ارسال پیام
    var channel = await connection.CreateChannelAsync();

    // آماده‌سازی پیام
    var message = JsonSerializer.Serialize(order);
    var body = Encoding.UTF8.GetBytes(message);

    // تنظیمات پیام (دائمی)
    var properties = new BasicProperties
    {
        Persistent = true
    };

    // ارسال پیام به صف
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "order_placed",
        mandatory: false,
        basicProperties: properties,
        body: body
    );

    Console.WriteLine($"✅ [Order API] پیام ارسال شد: {message}");

    await channel.CloseAsync();
    await channel.DisposeAsync();

    return Results.Accepted($"/order/{order.OrderId}", order);
});

app.MapGet("/", () => "Order API در حال اجرا است! 🚀");

app.Run();

public record Order(string OrderId, string ProductId, int Quantity);