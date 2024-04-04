using MediatR;

namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.NewOrder;

public class NewOrderHandler : IRequestHandler<NewOrderRequest, int>
{
    private readonly OrderingContext _orderingContext;

    public NewOrderHandler(OrderingContext orderingContext)
    {
        _orderingContext = orderingContext;
    }
    
    public async Task<int> Handle(NewOrderRequest request, CancellationToken cancellationToken)
    {
        // Create the order
        var address = new Address(request.Street, request.City, request.State, request.ZipCode, request.Country);
        var order = new Order(address);

        foreach (var item in request.OrderItems)
        {
            order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }

        _orderingContext.Orders.Add(order);
        
        await _orderingContext.SaveChangesAsync(cancellationToken);
        
        // Create or update the buyer details
        var cardTypeId = request.CardTypeId != 0 ? request.CardTypeId : 1;
        var buyer = await _orderingContext.Buyers
            .Where(b => b.IdentityGuid == request.UserId)
            .Include(b => b.PaymentMethods)
            .SingleOrDefaultAsync(cancellationToken);
        
        bool buyerOriginallyExisted = buyer != null;

        if (!buyerOriginallyExisted)
        {
            buyer = new Buyer
            {
                IdentityGuid = request.UserId,
                Name = request.UserName
            };
        }

        string alias = $"Payment Method on {DateTime.UtcNow}";
        PaymentMethod paymentMethod;
        var existingPayment = buyer.PaymentMethods
            .SingleOrDefault(p => p.CardTypeId == cardTypeId
                                  && p.CardNumber == request.CardNumber
                                  && p.Expiration == request.CardExpiration);

        if (existingPayment != null)
        {
            paymentMethod = existingPayment;
        }
        else
        {
            var payment = new PaymentMethod
            {
                CardNumber = request.CardNumber,
                SecurityNumber = request.CardSecurityNumber,
                CardHolderName = request.CardHolderName,
                Alias = alias,
                Expiration = request.CardExpiration,
                CardTypeId = cardTypeId
            };

            buyer.PaymentMethods.Add(payment);

            paymentMethod = payment;
        }

        if (buyerOriginallyExisted)
        {
            _orderingContext.Buyers.Update(buyer);
        }
        else
        {
            _orderingContext.Buyers.Add(buyer);
        }
        
        await _orderingContext.SaveChangesAsync(cancellationToken);
        
        // Update order details with buyer information
        order.Buyer = buyer;
        order.PaymentMethodId = paymentMethod.Id;
        
        _orderingContext.Orders.Update(order);
        
        await _orderingContext.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
