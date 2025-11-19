using OrderApi.Models;

namespace OrderApi.Services
{
    /// <summary>
    /// Service for checking application health
    /// </summary>
    public class HealthCheckService:IHealthCheckService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IConfiguration _configuration;

        public HealthCheckService(
            IMessagePublisher messagePublisher,
            ILogger<HealthCheckService> logger,
            IConfiguration configuration)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the current health status of the application
        /// </summary>
        public HealthResponse GetHealthStatus()
        {
            try
            {
                var isConnected = _messagePublisher.IsConnected();
                var connectionStatus = _messagePublisher.GetConnectionStatus();

                var status = isConnected ? "healthy" : "unhealthy";

                _logger.LogInformation(
                    "🏥 Health check performed - Status: {Status}, RabbitMQ: {RabbitMqStatus}",
                    status,
                    isConnected ? "Connected" : "Disconnected");

                return new HealthResponse
                {
                    Status = status,
                    RabbitMqConnected = isConnected,
                    RabbitMqHost = connectionStatus.Host,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error performing health check");

                return new HealthResponse
                {
                    Status = "unhealthy",
                    RabbitMqConnected = false,
                    RabbitMqHost = "unknown",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Checks if RabbitMQ connection is working
        /// </summary>
        public async Task<bool> CheckRabbitMqConnectionAsync()
        {
            try
            {
                var isConnected = _messagePublisher.IsConnected();

                if (isConnected)
                {
                    _logger.LogDebug("✅ RabbitMQ connection is healthy");
                }
                else
                {
                    _logger.LogWarning("⚠️ RabbitMQ connection is not available");
                }

                return await Task.FromResult(isConnected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error checking RabbitMQ connection");
                return false;
            }
        }
    }
}
