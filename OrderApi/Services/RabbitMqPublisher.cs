using System.Text;
using System.Text.Json;
using OrderApi.Models;
using RabbitMQ.Client;

namespace OrderApi.Services
{
    /// <summary>
    /// RabbitMQ implementation of the message publisher
    /// </summary>
    public class RabbitMqPublisher : IMessagePublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly string _queueName;
        private readonly string _host;
        private DateTime? _lastConnectedAt;
        private string? _lastError;

        private const string DefaultQueueName = "order_placed";

        public RabbitMqPublisher(
            IConnection connection,
            ILogger<RabbitMqPublisher> logger,
            IConfiguration configuration)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _queueName = configuration["RabbitMQ:QueueName"] ?? DefaultQueueName;
            _host = configuration["RabbitMQ:Host"]
                ?? Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                ?? "localhost";

            if (_connection.IsOpen)
            {
                _lastConnectedAt = DateTime.UtcNow;
                _logger.LogInformation("✅ RabbitMqPublisher initialized successfully");
            }
        }

        /// <summary>
        /// Publishes an order to RabbitMQ
        /// </summary>
        public async Task PublishOrderAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            try
            {
                // Validate connection
                if (!_connection.IsOpen)
                {
                    var error = "RabbitMQ connection is not open";
                    _logger.LogError(error);
                    _lastError = error;
                    throw new InvalidOperationException(error);
                }

                // Create channel
                var channel = await _connection.CreateChannelAsync();

                try
                {
                    // Declare queue (idempotent operation)
                    await channel.QueueDeclareAsync(
                        queue: _queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    // Serialize message
                    var message = JsonSerializer.Serialize(order, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    });
                    var body = Encoding.UTF8.GetBytes(message);

                    // Set message properties
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json",
                        Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                        MessageId = Guid.NewGuid().ToString(),
                        AppId = "OrderApi",
                        Headers = new Dictionary<string, object?>
                    {
                        { "order-id", order.OrderId },
                        { "product-id", order.ProductId },
                        { "created-at", order.CreatedAt.ToString("O") }
                    }
                    };

                    // Publish message
                    await channel.BasicPublishAsync(
                        exchange: string.Empty,
                        routingKey: _queueName,
                        mandatory: false,
                        basicProperties: properties,
                        body: body
                    );

                    _logger.LogInformation(
                        "✅ Order published successfully - OrderId: {OrderId}, ProductId: {ProductId}, Quantity: {Quantity}",
                        order.OrderId, order.ProductId, order.Quantity);

                    _lastError = null;
                }
                finally
                {
                    // Clean up channel
                    await channel.CloseAsync();
                    await channel.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                _logger.LogError(ex,
                    "❌ Failed to publish order - OrderId: {OrderId}",
                    order.OrderId);
                throw new InvalidOperationException(
                    $"Failed to publish order {order.OrderId}", ex);
            }
        }

        /// <summary>
        /// Checks if RabbitMQ connection is open
        /// </summary>
        public bool IsConnected()
        {
            return _connection?.IsOpen ?? false;
        }

        /// <summary>
        /// Gets detailed connection status
        /// </summary>
        public ConnectionStatus GetConnectionStatus()
        {
            return new ConnectionStatus
            {
                IsConnected = IsConnected(),
                Host = _host,
                LastConnectedAt = _lastConnectedAt,
                ErrorMessage = _lastError
            };
        }

        /// <summary>
        /// Disposes the connection
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_connection?.IsOpen == true)
                {
                    _logger.LogInformation("🔌 Closing RabbitMQ connection...");
                    _connection.CloseAsync();
                    _connection.Dispose();
                    _logger.LogInformation("✅ RabbitMQ connection closed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while disposing RabbitMQ connection");
            }
        }
    }
}
