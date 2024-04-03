namespace Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.EntityConfigurations;

class PaymentMethodEntityTypeConfiguration
    : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> paymentConfiguration)
    {
        paymentConfiguration.ToTable("paymentmethods", OrderingContext.DEFAULT_SCHEMA);

        paymentConfiguration.HasKey(b => b.Id);

        paymentConfiguration.Property(b => b.Id)
            .UseHiLo("paymentseq", OrderingContext.DEFAULT_SCHEMA);

        paymentConfiguration
            .Property(pm => pm.CardHolderName)
            .HasMaxLength(200)
            .IsRequired();

        paymentConfiguration
            .Property(pm => pm.Alias)
            .HasMaxLength(200)
            .IsRequired();

        paymentConfiguration
            .Property(pm => pm.CardNumber)
            .HasMaxLength(25)
            .IsRequired();

        paymentConfiguration
            .Property(pm => pm.Expiration)
            .HasMaxLength(25)
            .IsRequired();
        
        paymentConfiguration
            .HasOne(p => p.CardType)
            .WithMany();

        paymentConfiguration.Ignore(p => p.SecurityNumber);
    }
}
