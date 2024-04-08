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
    
    public async Task Handle(OrderStockRejectedEvent message, IMessageHandlerContext context)
    {
        var order = await _orderingContext.Orders.FindAsync([message.OrderId], context.CancellationToken);
        
        order.MarkOrderAsCancelled();

        await _orderingContext.SaveEntitiesAsync(context.CancellationToken);
    }
}