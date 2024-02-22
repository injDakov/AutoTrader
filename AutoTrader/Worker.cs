using System;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Business.Models.Dto;
using AT.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AT.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _services;

        public Worker(IServiceProvider services)
        {
            _services = services;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                await loggerService.CreateLogAsync(new LogDto(LogType.Info, "StopAsync", new DetailedMessage { Text = "The service has been stopped." }, @event: "Stopping"));
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();

            var traderService = scope.ServiceProvider.GetRequiredService<ITraderService>();

            await traderService.TriggerAsync(cancellationToken);
        }
    }
}