using MediatR;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Buyers.DomainEventHandlers;

public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler
    : INotificationHandler<OrderStartedDomainEvent>
{
    private readonly OrderingContext _orderingContext;

    public ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task Handle(OrderStartedDomainEvent request, CancellationToken cancellationToken)
    {
        var cardTypeId = request.CardTypeId != 0 ? request.CardTypeId : 1;
        var buyer = await _orderingContext.Buyers
            .Where(b => b.IdentityGuid == request.UserId)
            .Include(b => b.PaymentMethods)
            .SingleOrDefaultAsync(cancellationToken);
        
        bool buyerOriginallyExisted = buyer != null;

        if (!buyerOriginallyExisted)
        {
            buyer = new Buyer(request.UserId, request.UserName);
        }

        string alias = $"Payment Method on {DateTime.UtcNow}";
        buyer.VerifyOrAddPaymentMethod(
            request.CardNumber, 
            request.CardSecurityNumber, 
            request.CardHolderName,
            request.CardExpiration,
            cardTypeId,
            alias,
            request.Order.Id
            );

        if (buyerOriginallyExisted)
        {
            _orderingContext.Buyers.Update(buyer);
        }
        else
        {
            _orderingContext.Buyers.Add(buyer);
        }
        
        await _orderingContext.SaveEntitiesAsync(cancellationToken);
    }
}