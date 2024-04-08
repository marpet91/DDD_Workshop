namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Events;

public class OrderAwaitingValidationDomainEvent : INotification
{
    public Order Order { get; set; }
}