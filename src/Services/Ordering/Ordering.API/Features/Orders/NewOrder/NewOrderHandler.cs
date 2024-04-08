using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.Commands;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;

public class NewOrderHandler : IRequestHandler<NewOrderRequest, int>
{
    private readonly OrderingContext _orderingContext;

    public NewOrderHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task<int> Handle(NewOrderRequest request, CancellationToken cancellationToken)
    {
        // Create the order
        var order = new Order(request);

        var _ = _orderingContext.Orders.Add(order).Entity;
        
        await _orderingContext.SaveEntitiesAsync(cancellationToken);
        
        return order.Id;
    }
}
