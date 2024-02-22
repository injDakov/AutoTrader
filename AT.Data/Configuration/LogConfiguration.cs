using AT.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AT.Data.Configuration
{
    public class LogConfiguration : IEntityTypeConfiguration<Log>
    {
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