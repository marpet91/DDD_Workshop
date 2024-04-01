namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

public class PaymentMethod
    : Entity
{
    public string Alias { get; set; }
    public string CardNumber { get; set; }
    public string SecurityNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime Expiration { get; set; }

    public int CardTypeId { get; set; }
    public CardType CardType { get; set; }

    public int BuyerId { get; set; }

    public Buyer Buyer { get; set; }
}
