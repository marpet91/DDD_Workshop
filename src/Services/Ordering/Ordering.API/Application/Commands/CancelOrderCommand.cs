namespace Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;

public class CancelOrderCommand
{

    [DataMember]
    public int OrderNumber { get; set; }
}
