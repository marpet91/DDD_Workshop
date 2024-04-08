namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

class OrderItemEntityTypeConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> orderItemConfiguration)
    {
        orderItemConfiguration.ToTable("orderItems", OrderingContext.DEFAULT_SCHEMA);

        orderItemConfiguration.HasKey(o => o.Id);
        
        orderItemConfiguration.Ignore(b => b.DomainEvents);

        orderItemConfiguration.Property(o => o.Id)
            .UseHiLo("orderitemseq");

        orderItemConfiguration.Property<int>("OrderId")
            .IsRequired();

        orderItemConfiguration
            .Property(oi => oi.Discount)
            .IsRequired();

        orderItemConfiguration.Property(oi => oi.ProductId)
            .IsRequired();

        orderItemConfiguration
            .Property(oi => oi.ProductName)
            .IsRequired();

        orderItemConfiguration
            .Property(oi => oi.UnitPrice)
            .IsRequired();

        orderItemConfiguration
            .Property(oi => oi.Units)
            .IsRequired();

        orderItemConfiguration
            .Property(oi => oi.PictureUrl)
            .IsRequired(false);
    }
}
