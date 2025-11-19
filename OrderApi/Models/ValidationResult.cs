namespace OrderApi.Models
{
    /// <summary>
    /// Validation result
    /// </summary>
    public record ValidationResult
    {
        public bool IsValid { get; init; }
        public string? ErrorMessage { get; init; }
        public List<string> Errors { get; init; } = new();

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(string error) => new()
        {
            IsValid = false,
            ErrorMessage = error,
            Errors = new List<string> { error }
        };

        public static ValidationResult Failure(List<string> errors) => new()
        {
            IsValid = false,
            ErrorMessage = string.Join("; ", errors),
            Errors = errors
        };
    }
}
