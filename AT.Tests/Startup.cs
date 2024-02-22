using System.IO;
using System.Reflection;
using AT.Business.Interfaces;
using AT.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AT.Tests
{
    public class Startup
    {
        public void ConfigureHost(IHostBuilder hostBuilder)
        {
            hostBuilder
                 .ConfigureServices((context, services) =>
                 {
                     var configuration = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).AddJsonFile("appsettings.json").Build();

                     services.AddLogging();
                     services.AddSingleton<IConfiguration>(configuration);

                     services.AddTransient<ITraderHelperService, TraderHelperService>();
                 });
        }
    }
}
