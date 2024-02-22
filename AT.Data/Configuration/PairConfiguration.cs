using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    public class PairConfiguration : IEntityTypeConfiguration<Pair>
    {
        public void Configure(EntityTypeBuilder<Pair> builder)
        {
            builder.ToTable("Pairs");

            builder.Property(p => p.Id).UseIdentityColumn();
            builder.Property(p => p.CreateDate).IsRequired();
            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.OrderAmount).IsRequired().HasColumnType("decimal(18,5)");
            builder.Property(p => p.MaxOrderLevelCount).IsRequired();
            builder.Property(p => p.IsActive).IsRequired();

            builder.HasIndex(p => new { p.Name, p.Exchange }).IsUnique();
        }
    }
}