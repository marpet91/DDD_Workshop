namespace Microsoft.eShopOnContainers.Services.Ordering.API.DTOs;

public record OrderItemDto
{
    public string ProductName { get; set; }
    public int Units { get; set; }
    public decimal UnitPrice { get; set; }
    public string PictureUrl { get; set; }
}

public record OrderDto
{
    public int OrderNumber { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public decimal Total { get; set; }
}

public record OrderSummaryDto
{
    public int OrderNumber { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
    public decimal Total { get; set; }
}

public record CardTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}
