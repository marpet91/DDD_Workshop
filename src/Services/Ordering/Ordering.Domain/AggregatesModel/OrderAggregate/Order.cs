namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class Order
    : Entity
{
    protected Order() { }
    
    public Order(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        OrderStatusId = OrderStatus.Submitted.Id;
        OrderDate = DateTime.UtcNow;
    }

    public static Order NewDraft()
    {
        var order = new Order();
        return order;
    }

    public Address Address { get; private set; }
    
    public Buyer Buyer { get; set; }

    public int? BuyerId { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public int OrderStatusId { get; set; }

    public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public string Description { get; set; }

    public DateTime OrderDate { get; set; }

    public int? PaymentMethodId { get; set; }

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
            
            OrderItems.Add(orderItem);
        }
    }
}
