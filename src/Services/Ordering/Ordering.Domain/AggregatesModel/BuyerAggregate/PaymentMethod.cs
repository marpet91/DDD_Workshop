namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.AggregatesModel.BuyerAggregate;

public class PaymentMethod
    : Entity
{
    public PaymentMethod(string cardNumber, string securityNumber, string cardHolderName, DateTime expiration,
        int cardTypeId, string alias)
    {
        CardNumber = cardNumber ?? throw new ArgumentNullException(nameof(cardNumber));
        SecurityNumber = securityNumber ?? throw new ArgumentNullException(nameof(securityNumber));
        CardHolderName = cardHolderName ?? throw new ArgumentNullException(nameof(cardHolderName));
        Expiration = expiration;
        CardTypeId = cardTypeId;
        Alias = alias;
    }
    
    protected PaymentMethod () { }

    public string Alias { get; private set; }
    public string CardNumber { get; private set; }
    public string SecurityNumber { get; private set; }
    public string CardHolderName { get; private set; }
    public DateTime Expiration { get; private set; }

    public int CardTypeId { get; private set; }
    public CardType CardType { get; private set; }
}
