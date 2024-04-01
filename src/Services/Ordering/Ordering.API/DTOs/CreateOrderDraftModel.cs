namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public class CreateOrderDraftModel
{

    public string BuyerId { get; set; }

    public IEnumerable<BasketItem> Items { get; set; }
}
