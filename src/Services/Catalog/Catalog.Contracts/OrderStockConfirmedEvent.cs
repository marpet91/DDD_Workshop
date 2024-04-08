using NServiceBus.AttributeConventions.Contracts;

namespace Catalog.Contracts;

[Event]
public class OrderStockConfirmedEvent
{
    public int OrderId { get; set; }
}