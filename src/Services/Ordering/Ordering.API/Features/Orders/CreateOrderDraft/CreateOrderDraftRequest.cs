using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

public class CreateOrderDraftRequest : IRequest<OrderDraftModel>
{

    public string BuyerId { get; set; }

    public IEnumerable<ApiDto.BasketItem> Items { get; set; }

    public static OrderDraftModel FromOrder(Order order)
    {
        return new OrderDraftModel()
        {
            OrderItems = order.OrderItems.Select(oi => new OrderDraftModel.OrderItem
            {
                Discount = oi.Discount,
                ProductId = oi.ProductId,
                UnitPrice = oi.UnitPrice,
                PictureUrl = oi.PictureUrl,
                Units = oi.Units,
                ProductName = oi.ProductName
            }),
            Total = order.GetTotal()
        };
    }
}
