namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.OrderAggregate;

public class OrderItem
    : Entity
{
    public OrderItem(int productId, string productName, string pictureUrl, decimal unitPrice, decimal discount,
        int units)
    {
        ProductName = productName;
        PictureUrl = pictureUrl;
        UnitPrice = unitPrice;
        Discount = discount;
        Units = units;
        ProductId = productId;
    }

    public string ProductName { get; private set; }
    public string PictureUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public int Units { get; private set; }

    public int ProductId { get; private set; }
    
    public decimal GetPrice()
    {
        return Units * UnitPrice;
    }

    public void ApplyDiscount(decimal discount)
    {
        Discount = discount;
    }

    public void AddUnits(int units)
    {
        Units += units;
    }
}
