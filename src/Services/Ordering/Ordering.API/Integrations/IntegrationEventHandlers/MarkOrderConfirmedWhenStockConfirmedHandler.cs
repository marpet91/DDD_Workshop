using Catalog.Contracts;
using NServiceBus;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Integrations.IntegrationEventHandlers;

public class MarkOrderConfirmedWhenOrderConfirmedHandler : IHandleMessages<OrderStockConfirmedEvent>
{
    private readonly OrderingContext _orderingContext;

    public MarkOrderConfirmedWhenOrderConfirmedHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task Handle(OrderStockConfirmedEvent message, IMessageHandlerContext context)
    {
        var order = await _orderingContext.Orders.FindAsync([message.OrderId], context.CancellationToken);
        
        order.MarkOrderAsStockConfirmed();

        await _orderingContext.SaveEntitiesAsync(context.CancellationToken);
    }
}