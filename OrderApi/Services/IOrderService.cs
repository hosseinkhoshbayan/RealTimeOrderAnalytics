using OrderApi.Models;

namespace OrderApi.Services
{
    /// <summary>
    /// Interface for order operations
    /// </summary>
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(Order order);
        OrderStatsResponse GetStatistics();
    }
}
