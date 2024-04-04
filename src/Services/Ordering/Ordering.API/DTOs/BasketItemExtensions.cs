using Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public static class BasketItemExtensions
{
    public static OrderDraftModel.OrderItem ToOrderItemDTO(this BasketItem item)
    {
        return new OrderDraftModel.OrderItem()
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            PictureUrl = item.PictureUrl,
            UnitPrice = item.UnitPrice,
            Units = item.Quantity
        };
    }
}
