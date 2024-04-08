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
    
    public Task Handle(OrderAwaitingValidationDomainEvent notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}