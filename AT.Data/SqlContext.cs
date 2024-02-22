using AT.Domain;
using Microsoft.EntityFrameworkCore;

namespace AT.Data
{
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Pair> Pairs { get; set; }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(SqlContext).Assembly);
        }
    }
}