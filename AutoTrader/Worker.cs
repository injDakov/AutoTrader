using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using AT.Domain.Models;
using AT.Domain.Models.Dtos;
using AT.Worker.Extensions;
using AT.Worker.Interfaces;
using Bitfinex.Net;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.RestV1Objects;
using CryptoExchange.Net.Authentication;
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
        private IConfiguration _configuration;
        private BitfinexClient _bitfinexClient;
        private SqlContext _sqlContext;
        private ILoggerService _loggerService;

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
                _sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();
                _loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                await _loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StartAsync", "The service was started."));

                await UpdatePairs();
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
                _sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();

                _loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                await _loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, StopAsync", "The service was stopped."));
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

                    BitfinexClient.SetDefaultOptions(new BitfinexClientOptions
                    {
                        ApiCredentials =
                            new ApiCredentials(
                                "<Set your key>",
                                "<Set your secret>"),
                    });

                    _bitfinexClient = new BitfinexClient();

                    _sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();
                    _loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

                    var pairs = _sqlContext.Pairs.Where(p => p.IsActive).ToList();

                    decimal percentSellProfitLevel1 = 0; // Set your sell ratio for Level 1
                    decimal percentBuyProfitLevel1 = 0; // Set your buy ratio for Level 1

                    decimal percentSellProfitLevel2 = 0;
                    decimal percentBuyProfitLevel2 = 0;

                    decimal percentSellProfitLevel3 = 0;
                    decimal percentBuyProfitLevel3 = 0;

                    decimal percentSellProfitLevel4 = 0;
                    decimal percentBuyProfitLevel4 = 0;

                    var activeBitfinexOrders = await GetBitfinexActiveOrdersAsync(stoppingToken);

                    var activeDbOrders = _sqlContext.Orders.Where(o => o.Status == OrderStatus.Active.ToString());

                    if (DateTime.Now.Minute == 0)
                    {
                        await _loggerService.CreateLog(
                            new Log(
                                LogType.Info,
                                $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                $"Next iteration. We have {pairs.Count} active pairs with {pairs.Sum(p => p.MaxOrderLevel)} active orders.",
                                $"Active Sell orders {activeBitfinexOrders.Count(o => o.Amount < 0)}.{Environment.NewLine}Active Buy orders {activeBitfinexOrders.Count(o => o.Amount > 0)}."));
                    }

                    var pricesRequest = await GetBitfinexPricesAsync(pairs, stoppingToken);

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
                                var currentOrderHistory = await GetBitfinexOrderHistoryAsync(pair, activeDbOrder.OrderId, stoppingToken);

                                //var currentOrderHistory = bitfinexOrderHistoryForThisPair.FirstOrDefault(o => o.Id == activeDbOrder.OrderId);

                                if (currentOrderHistory == null)
                                {
                                    activeDbOrder.Status = OrderStatus.Unknown.ToString();

                                    await _loggerService.CreateLog(new Log(LogType.Warning, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory == null)", "activeOrderFromDB.Status = OrderStatus.Unknown.ToString()"));
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

                                                var newOrder = await PlaceNewOrderAsync(orderDB, pair, OrderSide.Buy, buyPrice, stoppingToken);

                                                if (newOrder != null)
                                                {
                                                    pair.PairHistory
                                                        .FirstOrDefault(ph => ph.IsActive && ph.OrderLevel == activeDbOrder.OrderLevel)
                                                        .ExecutedSellOrderCount += 1;

                                                    activeBitfinexOrdersCount += 1;

                                                    await _loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal < 0)", $"Place Buy ({pair.Name}) after Sell order."));
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

                                                var newOrder = await PlaceNewOrderAsync(orderDB, pair, OrderSide.Sell, sellPrice, stoppingToken);

                                                if (newOrder != null)
                                                {
                                                    pair.PairHistory
                                                        .FirstOrDefault(ph => ph.IsActive && ph.OrderLevel == activeDbOrder.OrderLevel)
                                                        .ExecutedBuyOrderCount += 1;

                                                    activeBitfinexOrdersCount += 1;

                                                    await _loggerService.CreateLog(new Log(LogType.Info, $"AutoTrader v{_assemblyVersion}, ExecuteAsync (orderHistory.AmountOriginal > 0)", $"Place Sell ({pair.Name}) after Buy order."));
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

                                await _sqlContext.SaveChangesAsync();
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

                            var newOrder = await PlaceNewOrderAsync(orderDB, pair, OrderSide.Sell, lastPrice * (decimal)orderDB.ProfitRatio, stoppingToken);

                            if (newOrder != null)
                            {
                                var log = new Log(
                                    LogType.Info,
                                    $"AutoTrader v{_assemblyVersion}, ExecuteAsync",
                                    $"Placed initial Sell({pair.Name}) order.",
                                    JsonConvert.SerializeObject(
                                        new DetailedMessage { NewOrder = JsonConvert.SerializeObject(newOrder, Formatting.Indented) }, Formatting.Indented));

                                await _loggerService.CreateLog(log);
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

        private async Task UpdatePairs()
        {
            try
            {
                var configPairs = _configuration.GetSection("Pairs").Get<List<PairDto>>();

                if (configPairs.GroupBy(p => p.Name).Any(x => x.Count() > 1))
                {
                    string msg = "There are duplicate pairs. Please check and restart the service!";

                    throw new Exception(msg);
                }

                if (configPairs.Any(p => string.IsNullOrEmpty(p.Name)))
                {
                    string msg = "There are pairs with empty value for name. Please check and restart the service!";

                    throw new Exception(msg);
                }

                if (configPairs.Any(p => p.OrderAmount == 0))
                {
                    string msg = $"The pair {configPairs.FirstOrDefault(p => p.OrderAmount == 0).Name} has empty value for OrderAmount. Please check and restart the service!";

                    throw new Exception(msg);
                }

                if (configPairs.Any(p => p.MaxOrderLevel < 1))
                {
                    string msg = $"The pair {configPairs.FirstOrDefault(p => p.MaxOrderLevel < 1).Name} has value smaller than 1 for MaxOrderLevel. Please check and restart the service!";

                    throw new Exception(msg);
                }

                var dbPairs = _sqlContext.Pairs;

                foreach (var dbPair in dbPairs.Where(p => p.IsActive))
                {
                    if (!configPairs.Any(pc => pc.Name == dbPair.Name))
                    {
                        DeactivatePairAndHistory(dbPair);
                    }
                }

                foreach (var pair in configPairs)
                {
                    var dbPair = dbPairs.FirstOrDefault(o => o.Name == pair.Name);

                    if (dbPair == null)
                    {
                        // Adding the new Pair from the config file to the database.
                        var newPair = new Pair
                        {
                            CreateDate = DateTime.UtcNow,

                            Name = pair.Name,
                            OrderAmount = pair.OrderAmount,
                            MaxOrderLevel = pair.MaxOrderLevel,

                            IsActive = pair.IsActive,
                        };

                        AddPairHistory(newPair);

                        _sqlContext.Pairs.Add(newPair);
                    }
                    else
                    {
                        if (dbPair.OrderAmount != pair.OrderAmount
                            || dbPair.MaxOrderLevel != pair.MaxOrderLevel
                            || dbPair.IsActive != pair.IsActive)
                        {
                            // Updating the existing DB pair with changed data from the actual config file.
                            dbPair.OrderAmount = pair.OrderAmount;

                            // TODO :
                            // Calculate active hours
                            // Update Pair History

                            dbPair.LastUpdateDate = DateTime.UtcNow;
                        }

                        if (dbPair.PairHistory.Count(ph => ph.IsActive) != pair.MaxOrderLevel)
                        {
                            if (dbPair.PairHistory.Count(ph => ph.IsActive) > pair.MaxOrderLevel)
                            {
                                dbPair.PairHistory
                                    .Where(ph => ph.OrderLevel > pair.MaxOrderLevel)
                                    .ToList()
                                    .ForEach(ph =>
                                    {
                                        ph.IsActive = false;
                                        ph.EndDate = DateTime.UtcNow;
                                        ph.ActiveHours = (int)(ph.EndDate - ph.StartDate).Value.TotalHours;
                                    });
                            }
                            else
                            {
                                for (int i = dbPair.PairHistory.Count(ph => ph.IsActive) + 1; i <= pair.MaxOrderLevel; i++)
                                {
                                    dbPair.PairHistory.Add(new PairHistory(dbPair.OrderAmount, i, true));
                                }
                            }

                            dbPair.MaxOrderLevel = pair.MaxOrderLevel;
                        }

                        if (dbPair.IsActive != pair.IsActive)
                        {
                            dbPair.IsActive = pair.IsActive;

                            if (pair.IsActive)
                            {
                                AddPairHistory(dbPair);
                            }
                            else
                            {
                                DeactivatePairAndHistory(dbPair);
                            }
                        }
                    }
                }

                _sqlContext.SaveChanges();
            }
            catch (Exception ex)
            {
                await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, UpdatePairs()", ex.Message));

                await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, UpdatePairs()", "The service has stopped incidentally."));

                throw ex;
            }
        }

        private static void DeactivatePairAndHistory(Pair dbPair)
        {
            // Set an inactive flag for a pair that doesn't exist in the actual config file
            // and for all relative PairHistory.
            // Calculate the active hours.

            dbPair.IsActive = false;
            dbPair.PairHistory
                .ToList()
                .ForEach(ph =>
                {
                    ph.IsActive = false;
                    ph.EndDate = DateTime.UtcNow;
                    ph.ActiveHours = (int)(ph.EndDate - ph.StartDate).Value.TotalHours;
                });

            dbPair.LastUpdateDate = DateTime.UtcNow;
        }

        private static void AddPairHistory(Pair dbPair)
        {
            for (int i = 1; i <= dbPair.MaxOrderLevel; i++)
            {
                dbPair.PairHistory.Add(new PairHistory(dbPair.OrderAmount, i, true));
            }
        }

        private async Task<IEnumerable<BitfinexOrder>> GetBitfinexActiveOrdersAsync(CancellationToken stoppingToken)
        {
            int activeOrdersRequestMaxTry = 5;

            do
            {
                var activeOrdersRequest = await _bitfinexClient.GetActiveOrdersAsync();

                if (!activeOrdersRequest.Success)
                {
                    activeOrdersRequestMaxTry -= 1;

                    if (activeOrdersRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexActiveOrdersAsync()", activeOrdersRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await Task.Delay(750 * 60 * 1, stoppingToken);
                }
                else
                {
                    return activeOrdersRequest.Data;
                }
            }
            while (true);
        }

        private async Task<IEnumerable<BitfinexSymbolOverview>> GetBitfinexPricesAsync(IEnumerable<Pair> pairs, CancellationToken stoppingToken)
        {
            int pricesRequestMaxTry = 5;

            do
            {
                var pricesRequest = await _bitfinexClient.GetTickerAsync(stoppingToken, $"symbols=fUSD,t{string.Join(",t", pairs.Select(p => p.Name))}");

                if (!pricesRequest.Success)
                {
                    pricesRequestMaxTry -= 1;

                    if (pricesRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexPricesAsync()", pricesRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await Task.Delay(750 * 60 * 1, stoppingToken);
                }
                else
                {
                    return pricesRequest.Data;
                }
            }
            while (true);
        }

        private async Task<BitfinexOrder> GetBitfinexOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken stoppingToken)
        {
            int ordersHistoryRequestMaxTry = 5;
            int ordersHistoryNullMaxTry = 5;

            do
            {
                var ordersHistoryRequest = await _bitfinexClient.GetOrderHistoryAsync($"t{pair.Name}");

                if (!ordersHistoryRequest.Success)
                {
                    ordersHistoryRequestMaxTry -= 1;

                    if (ordersHistoryRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexOrderHistoryAsync()", ordersHistoryRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await Task.Delay(750 * 60 * 1, stoppingToken);
                }
                else
                {
                    if (!ordersHistoryRequest.Data.Any(o => o.Id == activeOrderId))
                    {
                        ordersHistoryNullMaxTry -= 1;

                        if (ordersHistoryNullMaxTry == 0)
                        {
                            await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexOrderHistoryAsync()", "Null"));

                            return null;
                        }

                        await Task.Delay(750 * 60 * 1, stoppingToken);
                    }
                    else
                    {
                        return ordersHistoryRequest.Data.FirstOrDefault(o => o.Id == activeOrderId);
                    }
                }
            }
            while (true);
        }

        private async Task<Order> PlaceNewOrderAsync(Order orderDB, Pair pair, OrderSide orderSide, decimal orderPrice, CancellationToken stoppingToken)
        {
            int placeOrderRequestMaxTry = 5;

            do
            {
                var placeOrderRequest = await _bitfinexClient.PlaceOrderAsync(pair.Name, orderSide, OrderTypeV1.ExchangeLimit, pair.OrderAmount, orderPrice);

                if (!placeOrderRequest.Success)
                {
                    placeOrderRequestMaxTry -= 1;

                    if (placeOrderRequestMaxTry == 0 || placeOrderRequest.Error.Message.Contains("not enough exchange balance for"))
                    {
                        await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, PlaceNewOrderAsync()", placeOrderRequest.Error.Message));

                        return null;
                    }

                    await Task.Delay(750 * 60 * 1, stoppingToken);
                }
                else
                {
                    return await AddOrderToDB(placeOrderRequest.Data, orderDB);
                }
            }
            while (true);
        }

        private async Task<Order> AddOrderToDB(BitfinexPlacedOrder bitfinexPlacedOrder, Order orderDB)
        {
            try
            {
                var newOrder = new Order
                {
                    OrderId = bitfinexPlacedOrder.Id.ConvertToLong(),
                    ClientOrderId = bitfinexPlacedOrder.ClientOrderId,

                    CreateDate = bitfinexPlacedOrder.Timestamp,

                    OrderLevel = orderDB.OrderLevel,
                    Side = bitfinexPlacedOrder.Side.ToString(),

                    Amount = bitfinexPlacedOrder.RemainingAmount,
                    AmountOriginal = bitfinexPlacedOrder.OriginalAmount,

                    Price = bitfinexPlacedOrder.Price,
                    PriceAverage = bitfinexPlacedOrder.AverageExecutionPrice,
                    ProfitRatio = orderDB.ProfitRatio,

                    PreviousOrderExecutedPrice = orderDB?.PreviousOrderExecutedPrice,
                    CurrentMarketPrice = orderDB.CurrentMarketPrice,

                    PreviousOrderProfitRatio = orderDB.PreviousOrderProfitRatio,
                    PreviousOrderProfitRatioToCurrentPrice = orderDB.PreviousOrderProfitRatioToCurrentPrice,

                    Status = OrderStatus.Active.ToString(),

                    Symbol = bitfinexPlacedOrder.Symbol,
                    Type = bitfinexPlacedOrder.Type.ToString(),
                    Source = bitfinexPlacedOrder.Source,

                    PreviousOrderId = orderDB.Id != 0 ? orderDB.Id : (long?)null,
                };

                await _sqlContext.Orders.AddAsync(newOrder);
                await _sqlContext.SaveChangesAsync();

                return newOrder;
            }
            catch (Exception ex)
            {
                await _loggerService.CreateLog(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, AddOrderToDB()", ex.ToString()));

                throw new Exception("processed");
            }
        }
    }
}