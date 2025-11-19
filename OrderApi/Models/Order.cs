namespace OrderApi
{
    /// <summary>
    /// Represents an order in the system
    /// </summary>
    public record Order(string OrderId, string ProductId, int Quantity)
    {
        /// <summary>
        /// Timestamp when the order was created
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}