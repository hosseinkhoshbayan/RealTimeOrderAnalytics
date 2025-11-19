namespace OrderApi.Models
{
    /// <summary>
    /// Represents the status of the message broker connection
    /// </summary>
    public record ConnectionStatus
    {
        public bool IsConnected { get; init; }
        public string Host { get; init; } = string.Empty;
        public DateTime? LastConnectedAt { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
