using NServiceBus.AttributeConventions.Contracts;

namespace Catalog.Contracts;

[Event]
public class OrderStockRejectedEvent
{
    public int OrderId { get; set; }
}