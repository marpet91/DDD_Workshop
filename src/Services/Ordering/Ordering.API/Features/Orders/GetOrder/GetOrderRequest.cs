using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrder;

public class GetOrderRequest : IRequest<OrderDto>
{
    public int OrderId { get; set; }
}