using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;
using NServiceBus.TransactionalSession;
using Ordering.Contracts;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Integrations.DomainEventHandlers;

public class PublishOrderAwaitingValidationFromDomainEventHandler : INotificationHandler<OrderAwaitingValidationDomainEvent>
{
    private readonly ITransactionalSession _messageSession;

    public PublishOrderAwaitingValidationFromDomainEventHandler(ITransactionalSession messageSession)
    {
        _messageSession = messageSession;
    }
    
    public async Task Handle(OrderAwaitingValidationDomainEvent notification, CancellationToken cancellationToken)
    {
        var message = new OrderAwaitingValidationEvent
        {
            OrderId = notification.Order.Id,
            OrderItems = notification.Order.OrderItems.Select(item => new OrderAwaitingValidationEvent.OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Units
                }
            ).ToList()
        };

        await _messageSession.Publish(message, cancellationToken);
    }
}