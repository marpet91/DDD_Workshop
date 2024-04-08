using NServiceBus.AttributeConventions.Contracts;

namespace Ordering.Contracts;

[Event]
public class OrderAwaitingValidationEvent
{
    public class OrderItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public int OrderId { get; set; }
    public List<OrderItem> OrderItems { get; set; }
}