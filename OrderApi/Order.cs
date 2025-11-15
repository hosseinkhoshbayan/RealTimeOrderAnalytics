namespace OrderApi
{
    public record Order(string OrderId, string ProductId, int Quantity);
}