namespace OrderApi.Models
{
    /// <summary>
    /// Health check response
    /// </summary>
    public record HealthResponse
    {
        public string Status { get; init; } = string.Empty;
        public bool RabbitMqConnected { get; init; }
        public string? RabbitMqHost { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
