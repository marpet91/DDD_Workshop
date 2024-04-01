namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

public class ShipOrderCommand
{

    [DataMember]
    public int OrderNumber { get; private set; }

    public ShipOrderCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
