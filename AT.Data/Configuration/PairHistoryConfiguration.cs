using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    /// <summary>PairHistoryConfiguration class.</summary>
    public class PairHistoryConfiguration : IEntityTypeConfiguration<PairHistory>
    {
        /// <summary>Configures the entity of type <span class="typeparameter">TEntity</span>.</summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<PairHistory> builder)
        {
            builder.ToTable("PairHistory");

            builder.Property(ph => ph.Id).UseIdentityColumn();
            builder.Property(ph => ph.StartDate).IsRequired();
            builder.Property(ph => ph.OrderAmount).IsRequired().HasColumnType("decimal(18,5)");
            builder.Property(ph => ph.OrderLevel).IsRequired();
            builder.Property(ph => ph.ActiveHours).IsRequired();
            builder.Property(ph => ph.ExecutedSellOrderCount).IsRequired();
            builder.Property(ph => ph.ExecutedBuyOrderCount).IsRequired();
            builder.Property(ph => ph.IsActive).IsRequired();

            builder.HasOne(ph => ph.Pair).WithMany(p => p.PairHistory).HasForeignKey(ph => ph.PairId);
        }
    }
}