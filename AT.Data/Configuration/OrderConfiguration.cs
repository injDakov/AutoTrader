using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    /// <summary>OrderConfiguration class.</summary>
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        /// <summary>Configures the entity of type <span class="typeparameter">TEntity</span>.</summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.Property(o => o.Id).UseIdentityColumn();
            builder.Property(o => o.OrderId).IsRequired();
            builder.Property(o => o.CreateDate).IsRequired();
            builder.Property(o => o.Side).IsRequired();

            builder.Property(o => o.Amount).IsRequired().HasColumnType("decimal(18,5)");
            builder.Property(o => o.AmountOriginal).IsRequired().HasColumnType("decimal(18,5)");

            builder.Property(o => o.Price).IsRequired().HasColumnType("decimal(18,5)");
            builder.Property(o => o.PriceAverage).HasColumnType("decimal(18,5)");
            builder.Property(o => o.ProfitRatio).HasColumnType("decimal(18,5)");

            builder.Property(o => o.PreviousOrderExecutedPrice).HasColumnType("decimal(18,5)");
            builder.Property(o => o.CurrentMarketPrice).HasColumnType("decimal(18,5)");

            builder.Property(o => o.PreviousOrderProfitRatio).HasColumnType("decimal(18,5)");
            builder.Property(o => o.PreviousOrderProfitRatioToCurrentPrice).HasColumnType("decimal(18,5)");

            builder.Property(o => o.Status).IsRequired();
            builder.Property(o => o.Symbol).IsRequired();
            builder.Property(o => o.Type).IsRequired();

            builder.HasIndex(o => o.OrderId).IsUnique();

            builder.HasOne(o => o.PreviousOrder).WithOne().HasForeignKey<Order>(o => o.PreviousOrderId);
        }
    }
}