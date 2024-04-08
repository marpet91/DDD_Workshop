namespace Microsoft.eShopOnContainers.Services.Ordering.Domain.Commands;

public interface INewOrderCommand
{
    string City { get; }
    string Street { get; }
    string State { get; }
    string Country { get; }
    string ZipCode { get; }
    string CardNumber { get; }
    string CardHolderName { get; }
    DateTime CardExpiration { get; }
    string CardSecurityNumber { get; }
    int CardTypeId { get; }
    string Buyer { get; }
    IEnumerable<IOrderItem> OrderItems { get; }
    string UserId { get; }
    string UserName { get; set; }
    
    public interface IOrderItem
    {
        int ProductId { get; }
        string ProductName { get; }
        decimal UnitPrice { get; }
        decimal Discount { get; }
        int Units { get; }
        string PictureUrl { get; }
    }
}