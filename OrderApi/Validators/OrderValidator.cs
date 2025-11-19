using System.Text.RegularExpressions;
using OrderApi.Models;

namespace OrderApi.Validators
{
    /// <summary>
    /// Validator for Order objects with comprehensive business rules
    /// </summary>
    public static class OrderValidator
    {
        // Constants for validation rules
        private const int MinQuantity = 1;
        private const int MaxQuantity = 1000;
        private const int MaxOrderIdLength = 50;
        private const int MaxProductIdLength = 50;

        // Regex patterns for validation
        private static readonly Regex OrderIdPattern = new(@"^[A-Z]{3,4}-\d{3,6}$",
            RegexOptions.Compiled);
        private static readonly Regex ProductIdPattern = new(@"^PROD-[A-Z0-9]{3,10}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates an order with all business rules
        /// </summary>
        /// <param name="order">The order to validate</param>
        /// <returns>Validation result with errors if any</returns>
        public static ValidationResult Validate(Order order)
        {
            if (order == null)
            {
                return ValidationResult.Failure("Order cannot be null");
            }

            var errors = new List<string>();

            // Validate OrderId
            var orderIdErrors = ValidateOrderId(order.OrderId);
            errors.AddRange(orderIdErrors);

            // Validate ProductId
            var productIdErrors = ValidateProductId(order.ProductId);
            errors.AddRange(productIdErrors);

            // Validate Quantity
            var quantityErrors = ValidateQuantity(order.Quantity);
            errors.AddRange(quantityErrors);

            // Validate CreatedAt (should not be in future)
            if (order.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            {
                errors.Add("CreatedAt cannot be in the future");
            }

            return errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Quick validation that returns simple result
        /// </summary>
        public static (bool IsValid, string? ErrorMessage) ValidateQuick(Order order)
        {
            var result = Validate(order);
            return (result.IsValid, result.ErrorMessage);
        }

        /// <summary>
        /// Validates OrderId field
        /// </summary>
        private static List<string> ValidateOrderId(string? orderId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(orderId))
            {
                errors.Add("OrderId is required");
                return errors;
            }

            if (orderId.Length > MaxOrderIdLength)
            {
                errors.Add($"OrderId cannot exceed {MaxOrderIdLength} characters");
            }

            if (!OrderIdPattern.IsMatch(orderId))
            {
                errors.Add("OrderId must be in format: XXX-123456 or XXXX-123456 (e.g., ORD-001, SALE-123456)");
            }

            return errors;
        }

        /// <summary>
        /// Validates ProductId field
        /// </summary>
        private static List<string> ValidateProductId(string? productId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(productId))
            {
                errors.Add("ProductId is required");
                return errors;
            }

            if (productId.Length > MaxProductIdLength)
            {
                errors.Add($"ProductId cannot exceed {MaxProductIdLength} characters");
            }

            if (!ProductIdPattern.IsMatch(productId))
            {
                errors.Add("ProductId must be in format: PROD-XXXXX (e.g., PROD-123, PROD-ABC123)");
            }

            return errors;
        }

        /// <summary>
        /// Validates Quantity field
        /// </summary>
        private static List<string> ValidateQuantity(int quantity)
        {
            var errors = new List<string>();

            if (quantity < MinQuantity)
            {
                errors.Add($"Quantity must be at least {MinQuantity}");
            }

            if (quantity > MaxQuantity)
            {
                errors.Add($"Quantity cannot exceed {MaxQuantity}");
            }

            return errors;
        }

        /// <summary>
        /// Validates only the critical fields (for quick checks)
        /// </summary>
        public static bool ValidateCriticalFields(Order order)
        {
            return !string.IsNullOrWhiteSpace(order?.OrderId)
                && !string.IsNullOrWhiteSpace(order?.ProductId)
                && order.Quantity > 0;
        }

        /// <summary>
        /// Gets all validation rules as a readable list
        /// </summary>
        public static List<string> GetValidationRules()
        {
            return new List<string>
        {
            $"OrderId: Required, max {MaxOrderIdLength} chars, format: XXX-123456",
            $"ProductId: Required, max {MaxProductIdLength} chars, format: PROD-XXXXX",
            $"Quantity: Required, between {MinQuantity} and {MaxQuantity}",
            "CreatedAt: Cannot be in the future"
        };
        }
    }
}
