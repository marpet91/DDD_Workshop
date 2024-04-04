using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.ShipOrder;

public class ShipOrderHandler : IRequestHandler<ShipOrderRequest, bool>
{
    private readonly OrderingContext _orderingContext;

    public ShipOrderHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }

    public async Task<bool> Handle(ShipOrderRequest request, CancellationToken cancellationToken)
    {
        var orderToUpdate = await _orderingContext.Orders.FindAsync(request.OrderNumber);
        if (orderToUpdate == null)
        {
            return false;
        }

        orderToUpdate.OrderStatusId = OrderStatus.Shipped.Id;
        orderToUpdate.Description = "The order was shipped.";

        await _orderingContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}