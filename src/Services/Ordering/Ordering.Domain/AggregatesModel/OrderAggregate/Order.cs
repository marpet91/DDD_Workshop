using Microsoft.eShopOnContainers.Services.Ordering.Domain.Commands;
using Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class Order
    : Entity
{
    private readonly List<OrderItem> _orderItems = new();
    protected Order() { }
    
    public Order(INewOrderCommand request)
    {
        var address = new Address(request.Street, request.City, request.State, request.ZipCode, request.Country);

        Address = address;
        OrderStatusId = OrderStatus.Submitted.Id;
        OrderDate = DateTime.UtcNow;

        foreach (var item in request.OrderItems)
        {
            AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.Discount, item.PictureUrl, item.Units);
        }
        
        AddDomainEvent(new OrderStartedDomainEvent
        {
            Order = this,
            CardSecurityNumber = request.CardSecurityNumber,
            CardTypeId = request.CardTypeId,
            CardExpiration = request.CardExpiration,
            CardNumber = request.CardNumber,
            UserId = request.UserId,
            UserName = request.UserName,
            CardHolderName = request.CardHolderName
        });
    }


    public static Order NewDraft()
    {
        var order = new Order();
        return order;
    }

    public Address Address { get; private set; }
    
    public Buyer Buyer { get; private set; }

    public int? BuyerId { get; private set; }

    public OrderStatus OrderStatus { get; private set; }
    public int OrderStatusId { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public string Description { get; private set; }

    public DateTime OrderDate { get; private set; }

    public int? PaymentMethodId { get; private set; }

    public decimal GetTotal() 
        => OrderItems.Sum(orderItem => orderItem.GetPrice());

    public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount,
        string pictureUrl, int units = 1)
    {
        var existingOrderForProduct = OrderItems
            .SingleOrDefault(o => o.ProductId == productId);

        if (existingOrderForProduct != null)
        {
            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.ApplyDiscount(discount);
            }

            existingOrderForProduct.AddUnits(units);
        }
        else
        {
            var orderItem = new OrderItem(productId, productName, pictureUrl, unitPrice, discount, units);
            
            _orderItems.Add(orderItem);
        }
    }

    public void MarkOrderAsShipped()
    {
        OrderStatusId = OrderStatus.Shipped.Id;
        Description = "The order was shipped.";
    }

    public void MarkOrderAsCancelled()
    {
        OrderStatusId = OrderStatus.Cancelled.Id;
        Description = $"The order was cancelled.";
    }
    
    public void MarkOrderAsStockConfirmed()
    {
        OrderStatusId = OrderStatus.StockConfirmed.Id;
        Description = "All the items were confirmed with available stock.";
    }

    public void AssignBuyerDetails(Buyer buyer, PaymentMethod paymentMethod)
    {
        Buyer = buyer;
        PaymentMethodId = paymentMethod.Id;
        OrderStatusId = OrderStatus.AwaitingValidation.Id;
    }
}
