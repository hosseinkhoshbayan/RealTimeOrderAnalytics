namespace OrderApi.Models
{
    /// <summary>
/// Standard response for order operations
/// </summary>
    public record OrderResponse
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public Order? Data { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
