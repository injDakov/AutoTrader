using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AT.Data
{
    public class Initializer
    {
        public static void Initialize(SqlContext context)
        {
            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Any())
            {
                var migrator = context.Database.GetService<IMigrator>();

                foreach (var targetMigration in pendingMigrations)
                {
                    try
                    {
                        migrator.Migrate(targetMigration);
                    }
                    catch
                    {
                    }
                }
            }

            new Initializer().Seed(context);
        }

        public void Seed(SqlContext context)
        {
        }
    }
}