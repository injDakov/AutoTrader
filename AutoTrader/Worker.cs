using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Business.Models.AppSettings;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using Bitfinex.Net.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace AT.Worker
{
    /// <summary>Worker class.</summary>
    public class Worker : BackgroundService
    {
        private readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        private readonly AppSettings _appSettings;

        /// <summary>Initializes a new instance of the <see cref="Worker" /> class.</summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public Worker(IServiceProvider services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;

            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        /// <summary>Triggered when the application host is ready to start the service.</summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>The task.</returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                var dbOrderService = scope.ServiceProvider.GetRequiredService<IDbOrderService>();

                await loggerService.CreateLogAsync(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StartAsync", "The service was started."));

                await dbOrderService.UpdateDbPairsAsync(cancellationToken);
            }

            await base.StartAsync(cancellationToken);
        }

        /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>The task.</returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                await loggerService.CreateLogAsync(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StopAsync", "The service was stopped."));
            }

            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService">IHostedService</see> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">
        /// Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)">StopAsync(System.Threading.CancellationToken)</see> is called.
        /// </param>
        /// <returns>The task.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pricesQueue = new Queue<IEnumerable<BitfinexSymbolOverview>>();

            while (!stoppingToken.IsCancellationRequested)
            {
                var stopWatch = new System.Diagnostics.Stopwatch();

                stopWatch.Start();

                try
                {
                    using var scope = _services.CreateScope();

                    var sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();
                    var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                    var bitfinexService = scope.ServiceProvider.GetRequiredService<IBitfinexService>();

                    var sellLevels = _appSettings.ProfitPercents.SellLevels;

                    var buyLevels = _appSettings.ProfitPercents.BuyLevels;

                    if (sellLevels.Any(sl => sl == 0) || buyLevels.Any(bl => bl == 0))
                    {
                        string msg = $"Some of ProfitPercents has an invalid value. Please check and restart the service!";

                        throw new Exception(msg);
                    }

                    var pairs = sqlContext.Pairs.Where(p => p.IsActive).ToList();

                    var activeBitfinexOrders = await bitfinexService.GetBitfinexActiveOrdersAsync(stoppingToken);

                    var activeDbOrders = sqlContext.Orders.Where(o => o.Status == OrderStatus.Active.ToString());

                    if (DateTime.Now.Hour % _appSettings.HealthCheckIntervalInHours == 0 && DateTime.Now.Minute == 0)
                    {
                        await loggerService.CreateLogAsync(
                            new Log(
                                LogType.Info,
                                $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                $"Next iteration. We have {pairs.Count} active pairs with {pairs.Sum(p => p.MaxOrderLevel)} active orders.",
                                $"Active Sell orders {activeBitfinexOrders.Count(o => o.Amount < 0)}.{Environment.NewLine}Active Buy orders {activeBitfinexOrders.Count(o => o.Amount > 0)}."));
                    }

                    var pricesRequest = await bitfinexService.GetBitfinexPricesAsync(pairs, stoppingToken);

                    pricesQueue.Enqueue(pricesRequest);

                    if (pricesQueue.Count > _appSettings.PricesQueueSize)
                    {
                        pricesQueue.Dequeue();
                    }

                    foreach (var pair in pairs)
                    {
                        var lastPrice = pricesRequest.FirstOrDefault(d => d.Symbol.ToLower() == $"t{pair.Name}".ToLower()).LastPrice;
                        var previousPrice = pricesQueue.Peek().FirstOrDefault(d => d.Symbol.ToLower() == $"t{pair.Name}".ToLower()).LastPrice;

                        var activeBitfinexOrdersForThisPair = activeBitfinexOrders.Where(o => o.Symbol.ToLower() == $"t{pair.Name}".ToLower()).ToList();

                        var activeDbOrdersForThisPair = activeDbOrders.Where(o => o.Symbol.ToLower() == $"{pair.Name}".ToLower()).ToList();

                        var activeDbOrdersForThisPairCount = activeDbOrdersForThisPair.Count;

                        int activeBitfinexOrdersCount = activeBitfinexOrdersForThisPair.Count(oB => Math.Abs(oB.AmountOriginal) == pair.OrderAmount || activeDbOrdersForThisPair.Any(o => o.OrderId == oB.Id));

                        foreach (var activeDbOrder in activeDbOrdersForThisPair)
                        {
                            var activeBitfinexOrder = activeBitfinexOrdersForThisPair.FirstOrDefault(o => o.Id == activeDbOrder.OrderId);

                            if (activeBitfinexOrder == null)
                            {
                                var currentOrderHistory = await bitfinexService.GetBitfinexOrderHistoryAsync(pair, activeDbOrder.OrderId, stoppingToken);

                                if (currentOrderHistory == null)
                                {
                                    activeDbOrder.Status = OrderStatus.Unknown.ToString();

                                    await loggerService.CreateLogAsync(new Log(LogType.Warning, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (currentOrderHistory == null)", "activeOrderFromDB.Status = OrderStatus.Unknown.ToString()"));
                                }
                                else
                                {
                                    activeDbOrder.Status = currentOrderHistory.Status.ToString();

                                    if (currentOrderHistory.Status == OrderStatus.Canceled)
                                    {
                                        activeDbOrdersForThisPairCount -= 1;
                                    }
                                    else if (currentOrderHistory.Status == OrderStatus.Executed)
                                    {
                                        if (currentOrderHistory.PriceAverage != null)
                                        {
                                            decimal buyPrice = 0;
                                            decimal sellPrice = 0;
                                            decimal profitRatioBuy = 0;
                                            decimal profitRatioSell = 0;

                                            buyPrice = new List<decimal>()
                                                            {
                                                                (decimal)currentOrderHistory.PriceAverage * buyLevels[activeDbOrder.OrderLevel - 1],
                                                                lastPrice * buyLevels[activeDbOrder.OrderLevel - 1],
                                                                previousPrice * buyLevels[activeDbOrder.OrderLevel - 1],
                                                            }
                                                            .Min();

                                            sellPrice = new List<decimal>()
                                                            {
                                                                (decimal)currentOrderHistory.PriceAverage * sellLevels[activeDbOrder.OrderLevel - 1],
                                                                lastPrice * sellLevels[activeDbOrder.OrderLevel - 1],
                                                                previousPrice * sellLevels[activeDbOrder.OrderLevel - 1],
                                                            }
                                                            .Max();

                                            profitRatioBuy = buyLevels[activeDbOrder.OrderLevel - 1];
                                            profitRatioSell = sellLevels[activeDbOrder.OrderLevel - 1];

                                            var orderDB = new Order
                                            {
                                                OrderLevel = activeDbOrder.OrderLevel,
                                                CurrentMarketPrice = lastPrice,
                                                PreviousOrderExecutedPrice = currentOrderHistory.PriceAverage,
                                                PreviousOrderProfitRatio = activeDbOrder.ProfitRatio,
                                                Id = activeDbOrder.Id,
                                            };

                                            if (currentOrderHistory.AmountOriginal < 0)
                                            {
                                                // Place order for buy.

                                                orderDB.ProfitRatio = profitRatioBuy;

                                                if (activeDbOrder.PreviousOrder != null)
                                                {
                                                    if (activeDbOrder.CurrentMarketPrice.HasValue)
                                                    {
                                                        orderDB.PreviousOrderProfitRatioToCurrentPrice =
                                                            Math.Min(
                                                                lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price,
                                                                lastPrice / (decimal)activeDbOrder.CurrentMarketPrice);
                                                    }
                                                    else
                                                    {
                                                        orderDB.PreviousOrderProfitRatioToCurrentPrice = lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price;
                                                    }
                                                }

                                                var newOrder = await bitfinexService.PlaceNewOrderAsync(orderDB, pair, OrderSide.Buy, buyPrice, stoppingToken);

                                                if (newOrder != null)
                                                {
                                                    pair.PairHistory
                                                        .FirstOrDefault(ph => ph.IsActive && ph.OrderLevel == activeDbOrder.OrderLevel)
                                                        .ExecutedSellOrderCount += 1;

                                                    activeBitfinexOrdersCount += 1;

                                                    await loggerService.CreateLogAsync(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal < 0)", $"Place Buy ({pair.Name}) after Sell order."));
                                                }
                                            }
                                            else if (currentOrderHistory.AmountOriginal > 0)
                                            {
                                                // Place order for sell.

                                                orderDB.ProfitRatio = profitRatioSell;

                                                if (activeDbOrder.PreviousOrder != null)
                                                {
                                                    if (activeDbOrder.CurrentMarketPrice.HasValue)
                                                    {
                                                        orderDB.PreviousOrderProfitRatioToCurrentPrice =
                                                            Math.Max(
                                                                lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price,
                                                                lastPrice / (decimal)activeDbOrder.CurrentMarketPrice);
                                                    }
                                                    else
                                                    {
                                                        orderDB.PreviousOrderProfitRatioToCurrentPrice = lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price;
                                                    }
                                                }

                                                var newOrder = await bitfinexService.PlaceNewOrderAsync(orderDB, pair, OrderSide.Sell, sellPrice, stoppingToken);

                                                if (newOrder != null)
                                                {
                                                    pair.PairHistory
                                                        .FirstOrDefault(ph => ph.IsActive && ph.OrderLevel == activeDbOrder.OrderLevel)
                                                        .ExecutedBuyOrderCount += 1;

                                                    activeBitfinexOrdersCount += 1;

                                                    await loggerService.CreateLogAsync(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal > 0)", $"Place Sell ({pair.Name}) after Buy order."));
                                                }
                                            }
                                        }
                                    }

                                    activeDbOrder.ExecutedDate = currentOrderHistory.TimestampUpdated;
                                }

                                await sqlContext.SaveChangesAsync(stoppingToken);
                            }
                        }

                        if (pair.MaxOrderLevel > activeDbOrdersForThisPairCount && pair.MaxOrderLevel > activeBitfinexOrdersCount)
                        {
                            var currentOrderLevel = activeDbOrdersForThisPairCount + 1;

                            var orderDB = new Order
                            {
                                OrderLevel = currentOrderLevel,
                                CurrentMarketPrice = lastPrice,
                                ProfitRatio = sellLevels[currentOrderLevel],
                            };

                            var sellPrice = new List<decimal>()
                                                {
                                                        lastPrice * (decimal)orderDB.ProfitRatio,
                                                        previousPrice * (decimal)orderDB.ProfitRatio,
                                                }
                                                    .Max();

                            var newOrder = await bitfinexService.PlaceNewOrderAsync(orderDB, pair, OrderSide.Sell, sellPrice, stoppingToken);

                            if (newOrder != null)
                            {
                                var log = new Log(
                                    LogType.Info,
                                    $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                    $"Placed initial Sell({pair.Name}) order.",
                                    JsonConvert.SerializeObject(
                                        new DetailedMessage { NewOrder = JsonConvert.SerializeObject(newOrder, Formatting.Indented) }, Formatting.Indented));

                                await loggerService.CreateLogAsync(log);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message != "processed")
                    {
                        using var scope = _services.CreateScope();

                        var context = scope.ServiceProvider.GetRequiredService<SqlContext>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                        await logger.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (Main Try)", ex.Message));
                    }
                }

                stopWatch.Stop();

                await AddDelayBeforeNextIteration(stopWatch.ElapsedMilliseconds, stoppingToken);
            }
        }

        private async Task AddDelayBeforeNextIteration(long elapsedMilliseconds, CancellationToken stoppingToken)
        {
            int timeToNextIteration = 1000 * 60 * 1;

            if (elapsedMilliseconds > timeToNextIteration)
            {
                timeToNextIteration = 0;
            }
            else
            {
                timeToNextIteration -= (int)elapsedMilliseconds;
            }

            await Task.Delay(timeToNextIteration, stoppingToken);
        }
    }
}