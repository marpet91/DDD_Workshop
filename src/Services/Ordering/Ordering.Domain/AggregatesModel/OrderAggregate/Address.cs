using Microsoft.eShopOnContainers.Services.Ordering.Domain.SeedWork;

namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class Address
{
    public String Street { get; set; }
    public String City { get; set; }
    public String State { get; set; }
    public String Country { get; set; }
    public String ZipCode { get; set; }
}
