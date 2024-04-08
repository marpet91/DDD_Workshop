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
    
    public Task Handle(OrderStockConfirmedEvent message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}