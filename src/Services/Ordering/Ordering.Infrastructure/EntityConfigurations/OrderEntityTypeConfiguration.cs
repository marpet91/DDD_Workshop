namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> orderConfiguration)
    {
        orderConfiguration.ToTable("orders", OrderingContext.DEFAULT_SCHEMA);

        orderConfiguration.HasKey(o => o.Id);

        orderConfiguration.Property(o => o.Id)
            .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);

        orderConfiguration
            .OwnsOne(o => o.Address, a =>
            {
                a.Property<int>("OrderId")
                .UseHiLo("orderseq", OrderingContext.DEFAULT_SCHEMA);
                a.WithOwner();
            });
        
        orderConfiguration
            .Property(o => o.OrderDate)
            .IsRequired();

        orderConfiguration.HasOne(o => o.OrderStatus);
        
        orderConfiguration
            .Property(o => o.PaymentMethodId)
            .IsRequired(false);

        orderConfiguration
            .Property(o => o.Description)
            .IsRequired(false);

        orderConfiguration
            .HasMany(o => o.OrderItems)
            .WithOne();
        
        var navigation = orderConfiguration.Metadata.FindNavigation(nameof(Order.OrderItems));

        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        orderConfiguration.HasOne<PaymentMethod>()
            .WithMany()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        orderConfiguration.HasOne(o => o.Buyer)
            .WithMany()
            .IsRequired(false);

        orderConfiguration.HasOne(o => o.OrderStatus)
            .WithMany();
    }
}
