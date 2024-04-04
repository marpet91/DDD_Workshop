using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder;

public class GetOrderHandler : IRequestHandler<GetOrderRequest, OrderDto>
{
    private readonly OrderingContext _orderingContext;

    public GetOrderHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task<OrderDto> Handle(GetOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _orderingContext.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => o.Id == request.OrderId)
            .SingleOrDefaultAsync();

        if (order == null)
        {
            return null;
        }
            
        var queryOrder = new OrderDto
        {
            OrderNumber = order.Id,
            Description = order.Description,
            Street = order.Address.Street,
            City = order.Address.City,
            ZipCode = order.Address.ZipCode,
            Country = order.Address.Country,
            Date = order.OrderDate,
            Status = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(orderItem => new OrderDto.OrderItem
            {
                ProductName = orderItem.ProductName,
                PictureUrl = orderItem.PictureUrl,
                UnitPrice = orderItem.UnitPrice,
                Units = orderItem.Units
            }).ToList()
        };
        
        return queryOrder;
    }
}