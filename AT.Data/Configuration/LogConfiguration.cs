using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    /// <summary>LogConfiguration class.</summary>
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
        /// <summary>Configures the entity of type <span class="typeparameter">TEntity</span>.</summary>
        /// <param name="builder">The builder to be used to configure the entity type.</param>
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.ToTable("Logs");

            builder.Property(l => l.Id).UseIdentityColumn();
            builder.Property(l => l.CreateDate).IsRequired();
            builder.Property(l => l.Type).IsRequired();
            builder.Property(l => l.Action).IsRequired();
            builder.Property(l => l.Message).IsRequired();
        }
    }
}