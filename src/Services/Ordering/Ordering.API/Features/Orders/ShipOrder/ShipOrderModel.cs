using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.ShipOrder;

public class ShipOrderRequest : IRequest<bool>
{
    public int OrderNumber { get; set; }
}
