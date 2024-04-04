namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

public record OrderDraftModel
{
    public IEnumerable<OrderItem> OrderItems { get; set; }
    public decimal Total { get; set; }

    public record OrderItem
    {
        public int ProductId { get; init; }

        public string ProductName { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal Discount { get; init; }

        public int Units { get; init; }

        public string PictureUrl { get; init; }
    }
}