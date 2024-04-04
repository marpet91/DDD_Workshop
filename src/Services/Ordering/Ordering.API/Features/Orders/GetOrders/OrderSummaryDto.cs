namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

public record OrderSummaryDto
{
    public int OrderNumber { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
}