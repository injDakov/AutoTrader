using System;
using System.IO;
using System.Reflection;
using AT.Data;
using AT.Worker.Interfaces;
using AT.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AT.Worker
{
    /// <summary>Program class.</summary>
    public class Program
    {
        /// <summary>Defines the entry point of the application.</summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var appPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "logs\\LogsForDay.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(appPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var context = scope.ServiceProvider.GetService<SqlContext>();
                    Initializer.Initialize(context);
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occurred while initializing the database.");
                }
            }

            host.Run();
        }

        /// <summary>Creates the host builder.</summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var connection = hostContext.Configuration.GetSection("SqlSettings:ConnectionString").Value;
                    services.AddDbContext<SqlContext>(options => options.UseSqlServer(connection, x => x.MigrationsAssembly("AT.Data").EnableRetryOnFailure().CommandTimeout(180)));

                    services.AddTransient<ILoggerService, LoggerService>();

                    services.AddHostedService<Worker>();
                });
    }
}