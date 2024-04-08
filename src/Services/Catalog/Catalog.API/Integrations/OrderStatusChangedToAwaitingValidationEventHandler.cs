using Catalog.Contracts;
using NServiceBus;
using Ordering.Contracts;

namespace Microsoft.eShopOnContainers.Services.Catalog.API.Integrations;

public class OrderStatusChangedToAwaitingValidationEventHandler : IHandleMessages<OrderAwaitingValidationEvent>
{
    private readonly CatalogContext _catalogContext;

    public OrderStatusChangedToAwaitingValidationEventHandler(CatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }
    
    public Task Handle(OrderAwaitingValidationEvent message, IMessageHandlerContext context)
    {
        return Task.CompletedTask;
    }
}