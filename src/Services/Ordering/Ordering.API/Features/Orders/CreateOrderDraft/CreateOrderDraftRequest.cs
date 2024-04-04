using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CreateOrderDraft;

public class CreateOrderDraftRequest : IRequest<OrderDraftModel>
{

    public string BuyerId { get; set; }

    public IEnumerable<ApiDto.BasketItem> Items { get; set; }
}
