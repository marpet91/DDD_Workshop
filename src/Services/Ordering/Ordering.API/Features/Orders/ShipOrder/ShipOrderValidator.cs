namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.ShipOrder;

public class ShipOrderValidator : AbstractValidator<ShipOrderRequest>
{
    public ShipOrderValidator()
    {
        RuleFor(order => order.OrderNumber).NotEmpty().WithMessage("No orderId found");
    }   
}