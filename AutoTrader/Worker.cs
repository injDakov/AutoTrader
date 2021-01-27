using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Common.Extensions;
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

        /// <summary>Initializes a new instance of the <see cref="Worker" /> class.</summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public Worker(IServiceProvider services, IConfiguration configuration)
        {
            _services = services;
            _configuration = configuration;
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

                await loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StartAsync", "The service was started."));

                await dbOrderService.UpdateDbPairs();
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

                await loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StopAsync", "The service was stopped."));
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

                    decimal percentSellProfitLevel1 = _configuration["ProfitPercents:SellLevel1"].ConvertToDecimal();
                    decimal percentBuyProfitLevel1 = _configuration["ProfitPercents:BuyLevel1"].ConvertToDecimal();

                    decimal percentSellProfitLevel2 = _configuration["ProfitPercents:SellLevel2"].ConvertToDecimal();
                    decimal percentBuyProfitLevel2 = _configuration["ProfitPercents:BuyLevel2"].ConvertToDecimal();

                    decimal percentSellProfitLevel3 = _configuration["ProfitPercents:SellLevel3"].ConvertToDecimal();
                    decimal percentBuyProfitLevel3 = _configuration["ProfitPercents:BuyLevel3"].ConvertToDecimal();

                    decimal percentSellProfitLevel4 = _configuration["ProfitPercents:SellLevel4"].ConvertToDecimal();
                    decimal percentBuyProfitLevel4 = _configuration["ProfitPercents:BuyLevel4"].ConvertToDecimal();

                    if (percentSellProfitLevel1 == 0 ||
                        percentBuyProfitLevel1 == 0 ||
                        percentSellProfitLevel2 == 0 ||
                        percentBuyProfitLevel2 == 0 ||
                        percentSellProfitLevel3 == 0 ||
                        percentBuyProfitLevel3 == 0 ||
                        percentSellProfitLevel4 == 0 ||
                        percentBuyProfitLevel4 == 0)
                    {
                        string msg = $"Some of ProfitPercents has an invalid value. Please check and restart the service!";

                        throw new Exception(msg);
                    }

                    var pairs = sqlContext.Pairs.Where(p => p.IsActive).ToList();

                    var activeBitfinexOrders = await bitfinexService.GetBitfinexActiveOrdersAsync(stoppingToken);

                    var activeDbOrders = sqlContext.Orders.Where(o => o.Status == OrderStatus.Active.ToString());

                    if (DateTime.Now.Minute == 0)
                    {
                        await loggerService.CreateLog(
                            new Log(
                                LogType.Info,
                                $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                $"Next iteration. We have {pairs.Count} active pairs with {pairs.Sum(p => p.MaxOrderLevel)} active orders.",
                                $"Active Sell orders {activeBitfinexOrders.Count(o => o.Amount < 0)}.{Environment.NewLine}Active Buy orders {activeBitfinexOrders.Count(o => o.Amount > 0)}."));
                    }

                    var pricesRequest = await bitfinexService.GetBitfinexPricesAsync(pairs, stoppingToken);

                    foreach (var pair in pairs)
                    {
                        var lastPrice = pricesRequest.FirstOrDefault(d => d.Symbol.ToLower() == $"t{pair.Name}".ToLower()).LastPrice;

                        var activeBitfinexOrdersForThisPair = activeBitfinexOrders.Where(o => o.Symbol.ToLower() == $"t{pair.Name}".ToLower()).ToList();

                        var activeDbOrdersForThisPair = activeDbOrders.Where(o => o.Symbol.ToLower() == $"{pair.Name}".ToLower()).ToList();

                        int activeBitfinexOrdersCount = activeBitfinexOrdersForThisPair.Count(oB => Math.Abs(oB.Amount) == pair.OrderAmount || activeDbOrdersForThisPair.Any(o => o.OrderId == oB.Id));

                        foreach (var activeDbOrder in activeDbOrdersForThisPair)
                        {
                            var activeBitfinexOrder = activeBitfinexOrdersForThisPair.FirstOrDefault(o => o.Id == activeDbOrder.OrderId);

                            if (activeBitfinexOrder == null)
                            {
                                var currentOrderHistory = await bitfinexService.GetBitfinexOrderHistoryAsync(pair, activeDbOrder.OrderId, stoppingToken);

                                //var currentOrderHistory = bitfinexOrderHistoryForThisPair.FirstOrDefault(o => o.Id == activeDbOrder.OrderId);

                                if (currentOrderHistory == null)
                                {
                                    activeDbOrder.Status = OrderStatus.Unknown.ToString();

                                    await loggerService.CreateLog(new Log(LogType.Warning, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory == null)", "activeOrderFromDB.Status = OrderStatus.Unknown.ToString()"));
                                }
                                else
                                {
                                    // Should be update all properties, just in case.
                                    activeDbOrder.Status = currentOrderHistory.Status.ToString();

                                    if (currentOrderHistory.Status == OrderStatus.Executed)
                                    {
                                        if (currentOrderHistory.PriceAverage != null)
                                        {
                                            decimal buyPrice = 0;
                                            decimal sellPrice = 0;
                                            decimal profitRatioBuy = 0;
                                            decimal profitRatioSell = 0;

                                            switch (activeDbOrder.OrderLevel)
                                            {
                                                case 1:
                                                default:
                                                    buyPrice = Math.Min((decimal)currentOrderHistory.PriceAverage * percentBuyProfitLevel1, lastPrice * percentBuyProfitLevel1);
                                                    sellPrice = Math.Max((decimal)currentOrderHistory.PriceAverage * percentSellProfitLevel1, lastPrice * percentSellProfitLevel1);
                                                    profitRatioBuy = percentBuyProfitLevel1;
                                                    profitRatioSell = percentSellProfitLevel1;
                                                    break;

                                                case 2:
                                                    buyPrice = Math.Min((decimal)currentOrderHistory.PriceAverage * percentBuyProfitLevel2, lastPrice * percentBuyProfitLevel2);
                                                    sellPrice = Math.Max((decimal)currentOrderHistory.PriceAverage * percentSellProfitLevel2, lastPrice * percentSellProfitLevel2);
                                                    profitRatioBuy = percentBuyProfitLevel2;
                                                    profitRatioSell = percentSellProfitLevel2;
                                                    break;

                                                case 3:
                                                    buyPrice = Math.Min((decimal)currentOrderHistory.PriceAverage * percentBuyProfitLevel3, lastPrice * percentBuyProfitLevel3);
                                                    sellPrice = Math.Max((decimal)currentOrderHistory.PriceAverage * percentSellProfitLevel3, lastPrice * percentSellProfitLevel3);
                                                    profitRatioBuy = percentBuyProfitLevel3;
                                                    profitRatioSell = percentSellProfitLevel3;
                                                    break;

                                                case 4:
                                                    buyPrice = Math.Min((decimal)currentOrderHistory.PriceAverage * percentBuyProfitLevel4, lastPrice * percentBuyProfitLevel4);
                                                    sellPrice = Math.Max((decimal)currentOrderHistory.PriceAverage * percentSellProfitLevel4, lastPrice * percentSellProfitLevel4);
                                                    profitRatioBuy = percentBuyProfitLevel4;
                                                    profitRatioSell = percentSellProfitLevel4;
                                                    break;
                                            }

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
                                                // set order for buy

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

                                                    await loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal < 0)", $"Place Buy ({pair.Name}) after Sell order."));
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }

                                            if (currentOrderHistory.AmountOriginal > 0)
                                            {
                                                // set order for sell

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

                                                    await loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal > 0)", $"Place Sell ({pair.Name}) after Buy order."));
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }

                                    activeDbOrder.ExecutedDate = currentOrderHistory.TimestampUpdated;
                                }

                                await sqlContext.SaveChangesAsync();
                            }
                        }

                        if (pair.MaxOrderLevel > activeDbOrdersForThisPair.Count() && pair.MaxOrderLevel > activeBitfinexOrdersCount)
                        {
                            var orderDB = new Order
                            {
                                OrderLevel = activeDbOrdersForThisPair.Count() + 1,
                                CurrentMarketPrice = lastPrice,
                            };

                            switch (orderDB.OrderLevel)
                            {
                                case 1:
                                default:
                                    orderDB.ProfitRatio = percentSellProfitLevel1;
                                    break;

                                case 2:
                                    orderDB.ProfitRatio = percentSellProfitLevel2;
                                    break;

                                case 3:
                                    orderDB.ProfitRatio = percentSellProfitLevel3;
                                    break;

                                case 4:
                                    orderDB.ProfitRatio = percentSellProfitLevel4;
                                    break;
                            }

                            var newOrder = await bitfinexService.PlaceNewOrderAsync(orderDB, pair, OrderSide.Sell, lastPrice * (decimal)orderDB.ProfitRatio, stoppingToken);

                            if (newOrder != null)
                            {
                                var log = new Log(
                                    LogType.Info,
                                    $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                    $"Placed initial Sell({pair.Name}) order.",
                                    JsonConvert.SerializeObject(
                                        new DetailedMessage { NewOrder = JsonConvert.SerializeObject(newOrder, Formatting.Indented) }, Formatting.Indented));

                                await loggerService.CreateLog(log);
                            }
                            else
                            {
                                continue;
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

                        await logger.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (Main Try)", ex.Message));
                    }
                }

                stopWatch.Stop();

                int timeToNextIteration = 1000 * 60 * 1;

                if (stopWatch.ElapsedMilliseconds > timeToNextIteration)
                {
                    timeToNextIteration = 0;
                }
                else
                {
                    timeToNextIteration -= (int)stopWatch.ElapsedMilliseconds;
                }

                await Task.Delay(timeToNextIteration, stoppingToken);
            }
        }
    }
}