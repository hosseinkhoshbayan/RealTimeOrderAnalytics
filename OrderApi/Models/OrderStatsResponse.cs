namespace OrderApi.Models
{
    /// <summary>
    /// Order statistics response
    /// </summary>
    public record OrderStatsResponse
    {
        public int OrdersProcessed { get; init; }
        public int LastHour { get; init; }
        public double AverageQuantity { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
