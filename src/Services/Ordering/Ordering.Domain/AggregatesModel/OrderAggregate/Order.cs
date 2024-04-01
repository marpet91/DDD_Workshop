namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class Order
    : Entity
{
    public Address Address { get; set; }
    
    public Buyer Buyer { get; set; }

    public int? BuyerId { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public int OrderStatusId { get; set; }

    public bool IsDraft { get; set; }

    public ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public string Description { get; set; }

    public DateTime OrderDate { get; set; }

    public int? PaymentMethodId { get; set; }
}
