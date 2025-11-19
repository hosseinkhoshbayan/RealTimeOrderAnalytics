using System;
using System.Collections.Generic;
using System.Text;
using OrderApi;
using OrderApi.Validators;

namespace Tests
{
    public class OrderValidatorTests
    {
        [Fact]
        public void Validate_ValidOrder_ReturnsSuccess()
        {
            // Arrange
            var order = new Order("ORD-001", "PROD-123", 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_NullOrder_ReturnsFailure()
        {
            // Arrange
            Order? order = null;

            // Act
            var result = OrderValidator.Validate(order!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Order cannot be null", result.ErrorMessage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyOrderId_ReturnsFailure(string orderId)
        {
            // Arrange
            var order = new Order(orderId, "PROD-123", 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("OrderId is required", result.Errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("12345")]
        [InlineData("ORD")]
        public void Validate_InvalidOrderIdFormat_ReturnsFailure(string orderId)
        {
            // Arrange
            var order = new Order(orderId, "PROD-123", 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("format"));
        }

        [Theory]
        [InlineData("ORD-001")]
        [InlineData("SALE-12345")]
        [InlineData("CART-999999")]
        public void Validate_ValidOrderIdFormats_ReturnsSuccess(string orderId)
        {
            // Arrange
            var order = new Order(orderId, "PROD-123", 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyProductId_ReturnsFailure(string productId)
        {
            // Arrange
            var order = new Order("ORD-001", productId, 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("ProductId is required", result.Errors);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("123")]
        [InlineData("PRODUCT-123")]
        public void Validate_InvalidProductIdFormat_ReturnsFailure(string productId)
        {
            // Arrange
            var order = new Order("ORD-001", productId, 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("PROD-"));
        }

        [Theory]
        [InlineData("PROD-123")]
        [InlineData("PROD-ABC")]
        [InlineData("PROD-XYZ999")]
        public void Validate_ValidProductIdFormats_ReturnsSuccess(string productId)
        {
            // Arrange
            var order = new Order("ORD-001", productId, 5);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_QuantityLessThanOne_ReturnsFailure(int quantity)
        {
            // Arrange
            var order = new Order("ORD-001", "PROD-123", quantity);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("at least"));
        }

        [Theory]
        [InlineData(1001)]
        [InlineData(5000)]
        public void Validate_QuantityExceedsMax_ReturnsFailure(int quantity)
        {
            // Arrange
            var order = new Order("ORD-001", "PROD-123", quantity);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("cannot exceed"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(500)]
        [InlineData(1000)]
        public void Validate_ValidQuantity_ReturnsSuccess(int quantity)
        {
            // Arrange
            var order = new Order("ORD-001", "PROD-123", quantity);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_MultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var order = new Order("", "", 0);

            // Act
            var result = OrderValidator.Validate(order);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count >= 3);
            Assert.Contains(result.Errors, e => e.Contains("OrderId"));
            Assert.Contains(result.Errors, e => e.Contains("ProductId"));
            Assert.Contains(result.Errors, e => e.Contains("Quantity"));
        }

        [Fact]
        public void ValidateCriticalFields_ValidOrder_ReturnsTrue()
        {
            // Arrange
            var order = new Order("ORD-001", "PROD-123", 5);

            // Act
            var result = OrderValidator.ValidateCriticalFields(order);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateCriticalFields_InvalidOrder_ReturnsFalse()
        {
            // Arrange
            var order = new Order("", "", 0);

            // Act
            var result = OrderValidator.ValidateCriticalFields(order);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetValidationRules_ReturnsAllRules()
        {
            // Act
            var rules = OrderValidator.GetValidationRules();

            // Assert
            Assert.NotEmpty(rules);
            Assert.Contains(rules, r => r.Contains("OrderId"));
            Assert.Contains(rules, r => r.Contains("ProductId"));
            Assert.Contains(rules, r => r.Contains("Quantity"));
        }
    }
}
