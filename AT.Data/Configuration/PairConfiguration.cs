using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    /// <summary>PairConfiguration class.</summary>
    public class PairConfiguration : IEntityTypeConfiguration<Pair>
    {
        /// <summary>Configures the entity of type <span class="typeparameter">TEntity</span>.</summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Pair> builder)
        {
            builder.ToTable("Pairs");

            builder.Property(p => p.Id).UseIdentityColumn();
            builder.Property(p => p.CreateDate).IsRequired();
            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.OrderAmount).IsRequired().HasColumnType("decimal(18,5)");
            builder.Property(p => p.MaxOrderLevel).IsRequired();
            builder.Property(p => p.IsActive).IsRequired();

            builder.HasIndex(p => p.Name).IsUnique();
        }
    }
}