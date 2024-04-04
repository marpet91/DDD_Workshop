using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;

public class CancelOrderRequest : IRequest<bool>
{
    public int OrderNumber { get; set; }
}
