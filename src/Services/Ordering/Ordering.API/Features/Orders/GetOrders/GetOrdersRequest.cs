using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.GetOrders;

public class GetOrdersRequest : IRequest<IEnumerable<OrderSummaryDto>>
{
    
}