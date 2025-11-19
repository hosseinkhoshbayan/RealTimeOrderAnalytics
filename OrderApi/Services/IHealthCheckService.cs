using OrderApi.Models;

namespace OrderApi.Services
{
    /// <summary>
    /// Interface for health check operations
    /// </summary>
    public interface IHealthCheckService
    {
        HealthResponse GetHealthStatus();
        Task<bool> CheckRabbitMqConnectionAsync();
    }
}
