using AT.Domain;
using Microsoft.EntityFrameworkCore;

namespace AT.Data
{
    /// <summary>SqlContext class.</summary>
    public class SqlContext : DbContext
    {
        /// <summary>Initializes a new instance of the <see cref="SqlContext" /> class.</summary>
        /// <param name="options">The options for this context.</param>
        public SqlContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>Gets or sets the logs.</summary>
        /// <value>The logs.</value>
        public DbSet<Log> Logs { get; set; }

        /// <summary>Gets or sets the pairs.</summary>
        /// <value>The pairs.</value>
        public DbSet<Pair> Pairs { get; set; }

        /// <summary>Gets or sets the orders.</summary>
        /// <value>The orders.</value>
        public DbSet<Order> Orders { get; set; }

        /// <summary>Called when [model creating].</summary>
        /// <param name="builder">The builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(SqlContext).Assembly);
        }
    }
}