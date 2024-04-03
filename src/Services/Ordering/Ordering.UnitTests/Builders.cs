namespace UnitTest.Ordering;

using Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class AddressBuilder
{
    public Address Build()
    {
        return new Address
        {
            Street = "street",
            City = "city",
            State = "state",
            Country = "country",
            ZipCode = "zipcode"
        };
    }
}

public class OrderBuilder
{
    private readonly Order order;

    public OrderBuilder(Address address)
    {
        order = new Order
        {
            Address = address,
            OrderStatus = OrderStatus.Submitted,
            OrderDate = DateTime.UtcNow
        };
    }

    public OrderBuilder AddOne(
        int productId,
        string productName,
        decimal unitPrice,
        decimal discount,
        string pictureUrl,
        int units = 1)
    {
        OrderManager.AddOrderItem(order, productId, productName, unitPrice, discount, pictureUrl, units);
        return this;
    }

    public Order Build()
    {
        return order;
    }
}
