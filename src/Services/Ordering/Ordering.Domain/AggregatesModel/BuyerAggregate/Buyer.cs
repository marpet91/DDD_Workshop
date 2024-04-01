namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

public class Buyer
    : Entity
{
    public string IdentityGuid { get; set; }

    public string Name { get; set; }

    public ICollection<PaymentMethod> PaymentMethods { get; } = new List<PaymentMethod>();

    public ICollection<Order> Orders { get; } = new List<Order>();
}
