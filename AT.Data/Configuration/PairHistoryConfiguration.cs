using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    public class PairHistoryConfiguration : IEntityTypeConfiguration<PairHistory>
    {
        public void Configure(EntityTypeBuilder<PairHistory> builder)
        {
            builder.ToTable("PairHistory");

            builder.Property(ph => ph.Id).UseIdentityColumn();
            builder.Property(ph => ph.CreateDate).IsRequired();
            builder.Property(ph => ph.ActiveHours).IsRequired();
            builder.Property(ph => ph.ExecutedSellOrderCount).IsRequired();
            builder.Property(ph => ph.ExecutedBuyOrderCount).IsRequired();
            builder.Property(ph => ph.IsActive).IsRequired();

            builder.HasIndex(ph => new { ph.IsActive, ph.PairId }).IsUnique();

            builder.HasOne(ph => ph.Pair).WithOne(p => p.PairHistory).HasForeignKey<PairHistory>(ph => ph.PairId);
        }
    }
}