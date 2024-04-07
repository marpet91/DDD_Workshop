namespace Microsoft.eShopOnContainers.Services.Ordering.API.Features.Orders.CancelOrder;

public class CancelOrderValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderValidator()
    {
        RuleFor(order => order.OrderNumber).NotEmpty().WithMessage("No orderId found");
    }
}