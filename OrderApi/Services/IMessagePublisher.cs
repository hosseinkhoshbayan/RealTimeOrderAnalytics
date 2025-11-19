using System.Data;
using OrderApi.Models;

namespace OrderApi.Services
{
    /// <summary>
    /// Interface for publishing messages to a message broker
    /// </summary>
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes an order to the message queue
        /// </summary>
        /// <param name="order">The order to publish</param>
        /// <returns>Task representing the async operation</returns>
        Task PublishOrderAsync(Order order);

        /// <summary>
        /// Checks if the connection to the message broker is active
        /// </summary>
        /// <returns>True if connected, false otherwise</returns>
        bool IsConnected();

        /// <summary>
        /// Gets the current connection status details
        /// </summary>
        /// <returns>Connection status information</returns>
        ConnectionStatus GetConnectionStatus();
    }
}
