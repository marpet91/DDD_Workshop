using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

public class CreateOrderDraftHandler : IRequestHandler<CreateOrderDraftRequest, OrderDraftModel>
{
    public Task<OrderDraftModel> Handle(CreateOrderDraftRequest request, CancellationToken cancellationToken)
    {
        var order = new Order();
        var orderItems = request.Items.Select(i => i.ToOrderItemDTO());
        foreach (var item in orderItems)
        {
            OrderManager.AddOrderItem(order, item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        var result = OrderDraftModel.FromOrder(order);

        return Task.FromResult(result);
    }
}