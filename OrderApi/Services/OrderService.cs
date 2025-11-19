using OrderApi.Models;
using OrderApi.Validators;

namespace OrderApi.Services
{
    /// <summary>
/// Service for handling order business logic
/// </summary>
    public class OrderService : IOrderService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<OrderService> _logger;
        private static int _orderCounter = 0;
        private static readonly DateTime _serviceStartTime = DateTime.UtcNow;

        public OrderService(
            IMessagePublisher messagePublisher,
            ILogger<OrderService> logger)
        {
            _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates and publishes an order
        /// </summary>
        public async Task<OrderResponse> CreateOrderAsync(Order order)
        {
            try
            {
                _logger.LogInformation(
                    "📥 Processing order request - OrderId: {OrderId}",
                    order.OrderId);

                // Validate order
                var validationResult = OrderValidator.Validate(order);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "⚠️ Order validation failed - OrderId: {OrderId}, Errors: {Errors}",
                        order.OrderId,
                        string.Join(", ", validationResult.Errors));

                    return new OrderResponse
                    {
                        Success = false,
                        Message = validationResult.ErrorMessage,
                        Data = null
                    };
                }

                // Check message broker connection
                if (!_messagePublisher.IsConnected())
                {
                    _logger.LogError(
                        "❌ Message broker not connected - OrderId: {OrderId}",
                        order.OrderId);

                    return new OrderResponse
                    {
                        Success = false,
                        Message = "Message broker is not available. Please try again later.",
                        Data = null
                    };
                }

                // Publish order
                await _messagePublisher.PublishOrderAsync(order);

                // Increment counter
                Interlocked.Increment(ref _orderCounter);

                _logger.LogInformation(
                    "✅ Order processed successfully - OrderId: {OrderId}, Total Orders: {TotalOrders}",
                    order.OrderId,
                    _orderCounter);

                return new OrderResponse
                {
                    Success = true,
                    Message = "Order accepted and queued for processing",
                    Data = order
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Unexpected error processing order - OrderId: {OrderId}",
                    order.OrderId);

                return new OrderResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred while processing your order",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Gets order statistics
        /// </summary>
        public OrderStatsResponse GetStatistics()
        {
            var uptime = DateTime.UtcNow - _serviceStartTime;
            var ordersPerHour = uptime.TotalHours > 0
                ? (int)(_orderCounter / uptime.TotalHours)
                : _orderCounter;

            _logger.LogInformation("📊 Statistics requested - Total Orders: {TotalOrders}", _orderCounter);

            return new OrderStatsResponse
            {
                OrdersProcessed = _orderCounter,
                LastHour = ordersPerHour,
                AverageQuantity = Random.Shared.Next(3, 10), // Mock data
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
