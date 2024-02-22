using System;
using System.IO;
using System.Reflection;
using AT.Business.AutoMapper;
using AT.Business.Interfaces;
using AT.Business.Services;
using AT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace AT.Worker
{
    public class Program
    {
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
                    logger.LogError(e, "An error was encountered during the initialization of the database.");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    var connection = hostContext.Configuration.GetSection("SqlSettings:ConnectionString").Value;
                    services.AddDbContext<SqlContext>(options => options.UseSqlServer(connection, x => x.MigrationsAssembly("AT.Data").EnableRetryOnFailure().CommandTimeout(180)));

                    services.AddAutoMapper(typeof(ExchangeToPrivateProfile).Assembly);

                    services.AddTransient<ILoggerService, LoggerService>();
                    services.AddTransient<IDbService, DbService>();
                    services.AddTransient<ITraderService, TraderService>();
                    services.AddTransient<ITraderHelperService, TraderHelperService>();

                    services.AddHostedService<Worker>();
                });
    }
}