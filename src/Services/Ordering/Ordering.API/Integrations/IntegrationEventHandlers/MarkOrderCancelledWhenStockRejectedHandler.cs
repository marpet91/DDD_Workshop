using Catalog.Contracts;
using NServiceBus;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Integrations.IntegrationEventHandlers;

public class MarkOrderCancelledWhenStockRejectedHandler : IHandleMessages<OrderStockRejectedEvent>
{
    private readonly OrderingContext _orderingContext;

    public MarkOrderCancelledWhenStockRejectedHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public Task Handle(OrderStockRejectedEvent message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}