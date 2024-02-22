using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Enums;
using AT.Business.Helpers;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Business.Models.AppSettings;
using AT.Business.Models.Dto;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AT.Business.Services
{
    public class TraderService : ITraderService
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly IDbService _dbService;
        private readonly IMapper _mapper;
        private readonly ITraderHelperService _traderHelperService;

        private readonly AppSettings _appSettings;

        public TraderService(IServiceProvider services, IConfiguration configuration, ILoggerService loggerService, IDbService dbService, IMapper mapper, ITraderHelperService traderHelperService)
        {
            _services = services;
            _configuration = configuration;
            _loggerService = loggerService;
            _dbService = dbService;
            _mapper = mapper;
            _traderHelperService = traderHelperService;

            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public async Task TriggerAsync(CancellationToken cancellationToken)
        {
            IExchangeService bitfinexExchange = new BitfinexService(_configuration, _loggerService, _dbService, _mapper);

            IExchangeService binanceExchange = new BinanceService(_configuration, _loggerService, _dbService, _mapper);

            var tasks = new List<Task>();

            var listOfActiveExchanges = new List<string>();

            var updatePairsSummary = await _dbService.UpdatePairsConfigurationAsync(cancellationToken);

            if (_appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "bitfinex").IsActive)
            {
                listOfActiveExchanges.Add($"'{Exchange.Bitfinex}'");

                tasks.Add(DoWorkAsync(bitfinexExchange, Exchange.Bitfinex, cancellationToken));
            }

            if (_appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "binance").IsActive)
            {
                listOfActiveExchanges.Add($"'{Exchange.Binance}'");

                tasks.Add(DoWorkAsync(binanceExchange, Exchange.Binance, cancellationToken));
            }

            await SendInitNotification(listOfActiveExchanges, updatePairsSummary);
            await Task.WhenAll(tasks);
        }

        private async Task FillPricesQueueAsync(IExchangeService exchangeService, Queue<IEnumerable<Models.Exchange.SymbolOverview>> pricesQueue, IEnumerable<Domain.Pair> pairs, CancellationToken cancellationToken)
        {
            var pricesRequest = await exchangeService.GetPricesAsync(pairs, cancellationToken);

            pricesQueue.Enqueue(pricesRequest);

            if (pricesQueue.Count > _appSettings.PricesQueueSize)
            {
                pricesQueue.Dequeue();
            }
        }

        private IEnumerable<decimal> GetPairPriceFromQueue(Queue<IEnumerable<Models.Exchange.SymbolOverview>> pricesQueue, string pairName)
        {
            return pricesQueue.ToList()
                        .SelectMany(pricesQueueItems => pricesQueueItems)
                        .Where(d => d.Symbol.ToLower() == $"t{pairName}".ToLower())
                        .Select(a => a.LastPrice);
        }

        private async Task DoWorkAsync(IExchangeService exchangeService, Exchange exchangeType, CancellationToken cancellationToken)
        {
            var pricesQueue = new Queue<IEnumerable<Models.Exchange.SymbolOverview>>();

            var cacheKeyPairsName = $"{exchangeType}_Pairs";
            MemoryCache cachedPairs = new MemoryCache(cacheKeyPairsName);

            var cacheKeyActiveDbOrdersName = $"{exchangeType}_ActiveDbOrders";
            MemoryCache cachedActiveDbOrders = new MemoryCache(cacheKeyActiveDbOrdersName);

            bool cacheIsUpToDate = false;

            while (!cancellationToken.IsCancellationRequested)
            {
                var stopWatch = new System.Diagnostics.Stopwatch();

                stopWatch.Start();

                try
                {
                    using var scope = _services.CreateScope();

                    var sqlContext = scope.ServiceProvider.GetRequiredService<SqlContext>();

                    var pairs = GetPairsFromDB(cacheKeyPairsName, cachedPairs, sqlContext, exchangeType);

                    var activeExchangeOrders = await exchangeService.GetActiveOrdersAsync(cancellationToken);

                    var activeDbOrders = GetActiveDbOrdersFromDB(ref cacheIsUpToDate, cacheKeyActiveDbOrdersName, cachedActiveDbOrders, sqlContext, exchangeType);

                    if (HealthCheckHelper.IsItTime(DateTime.UtcNow, _appSettings.HealthCheckIntervalInHours, _appSettings.HealthCheckHours))
                    {
                        await _loggerService.CreateLogAsync(
                            new LogDto(
                                LogType.Info,
                                $"DoWorkAsync({exchangeType})",
                                new DetailedMessage { Text = $"{exchangeType} Next iteration. We have {pairs.Count()} active pairs with {pairs.Sum(p => p.MaxOrderLevelCount)} active orders." + "   " + $"Active Sell orders {activeDbOrders.Count(o => o.Side == "Sell")}.{Environment.NewLine}Active Buy orders {activeDbOrders.Count(o => o.Side == "Buy")}." },
                                @event: $"{exchangeType} HealthCheck"));

                        // Rough fix for avoiding multiple HealthCheck emails,
                        // caused because the time for iteration is less than 1 minute.
                        await Task.Delay(1000 * (60 - DateTime.UtcNow.Second), cancellationToken);
                    }

                    await FillPricesQueueAsync(exchangeService, pricesQueue, pairs, cancellationToken);

                    foreach (var pair in pairs)
                    {
                        var previousPrices = GetPairPriceFromQueue(pricesQueue, pair.Name);

                        var activeExchangeOrdersForThisPair = activeExchangeOrders.Where(o => o.Symbol.ToLower() == $"t{pair.Name}".ToLower() && Math.Abs(o.AmountOriginal) == pair.OrderAmount).ToList();

                        var activeDbOrdersForThisPair = activeDbOrders.Where(o => o.Symbol.ToLower().Contains($"{pair.Name}".ToLower()));

                        var activeDbOrdersForThisPairCount = activeDbOrdersForThisPair.Count();

                        int activeExchangeOrdersCount = activeExchangeOrdersForThisPair.Count(oB => Math.Abs(oB.AmountOriginal) == pair.OrderAmount || activeDbOrdersForThisPair.Any(o => o.OrderId == oB.Id));

                        var activeExchangeOrdersIsUpToDate = true;

                        foreach (var activeDbOrder in activeDbOrdersForThisPair)
                        {
                            if (!activeExchangeOrdersIsUpToDate)
                            {
                                await FillPricesQueueAsync(exchangeService, pricesQueue, pairs, cancellationToken);

                                previousPrices = GetPairPriceFromQueue(pricesQueue, pair.Name);

                                activeExchangeOrders = await exchangeService.GetActiveOrdersAsync(cancellationToken);
                                activeExchangeOrdersForThisPair = activeExchangeOrders.Where(o => o.Symbol.ToLower() == $"t{pair.Name}".ToLower() && Math.Abs(o.AmountOriginal) == pair.OrderAmount).ToList();

                                activeExchangeOrdersCount = activeExchangeOrdersForThisPair.Count(oB => Math.Abs(oB.AmountOriginal) == pair.OrderAmount || activeDbOrdersForThisPair.Any(o => o.OrderId == oB.Id));

                                activeExchangeOrdersIsUpToDate = true;
                            }

                            var activeExchangeOrder = activeExchangeOrdersForThisPair.FirstOrDefault(o => o.Id == activeDbOrder.OrderId);

                            if (activeExchangeOrder == null)
                            {
                                var currentExchangeOrderHistory = await exchangeService.GetOrderHistoryAsync(pair, activeDbOrder.OrderId, cancellationToken);

                                if (currentExchangeOrderHistory == null)
                                {
                                    activeDbOrder.Status = Models.Exchange.OrderStatusEnum.Unknown.ToString();

                                    var log = new LogDto(
                                       LogType.Warning,
                                       $"{exchangeType} ExecuteAsync",
                                       new DetailedMessage { Text = $"A temporary error appeared while getting the OrderHistor from the Trader API." });

                                    await _loggerService.CreateLogAsync(log);
                                }
                                else
                                {
                                    activeDbOrder.Status = currentExchangeOrderHistory.Status.ToString();

                                    if (currentExchangeOrderHistory.Status == Models.Exchange.OrderStatusEnum.Canceled)
                                    {
                                        activeDbOrdersForThisPairCount -= 1;
                                        cacheIsUpToDate = false;
                                    }
                                    else if (currentExchangeOrderHistory.Status == Models.Exchange.OrderStatusEnum.Executed)
                                    {
                                        activeDbOrder.ExecutedDate = currentExchangeOrderHistory.TimestampUpdated;

                                        if (currentExchangeOrderHistory.PriceAverage != null)
                                        {
                                            var orderDB = new Order
                                            {
                                                Exchange = exchangeType,
                                                CurrentMarketPrice = previousPrices.Last(),
                                                ProfitRatio = _appSettings.ProfitRatio,
                                                PreviousOrderExecutedPrice = currentExchangeOrderHistory.PriceAverage,
                                                PreviousOrderProfitRatio = activeDbOrder.ProfitRatio,
                                                Id = activeDbOrder.Id,
                                            };

                                            if (currentExchangeOrderHistory.AmountOriginal < 0)
                                            {
                                                var potentialNewPrice = new List<decimal>()
                                                            {
                                                                (decimal)currentExchangeOrderHistory.PriceAverage / _appSettings.ProfitRatio,
                                                                previousPrices.Min() / _appSettings.ProfitRatio,
                                                            }
                                                            .Min();

                                                var alltOrderPrices = activeExchangeOrdersForThisPair.Select(o => o.Price);

                                                orderDB.Price = _traderHelperService.CalculateNewOrderPrice(Models.Exchange.OrderSideEnum.Buy, potentialNewPrice, alltOrderPrices);

                                                var newOrder = await PrepareToPlaceOrderAsync(exchangeService, pair, orderDB, activeDbOrder, Models.Exchange.OrderSideEnum.Buy, exchangeType, cancellationToken);

                                                cacheIsUpToDate = false;

                                                if (newOrder != null)
                                                {
                                                    activeExchangeOrdersCount++;
                                                    activeDbOrdersForThisPairCount++;
                                                    activeExchangeOrdersIsUpToDate = false;
                                                }
                                            }
                                            else if (currentExchangeOrderHistory.AmountOriginal > 0)
                                            {
                                                var potentialNewPrice = new List<decimal>()
                                                            {
                                                                (decimal)currentExchangeOrderHistory.PriceAverage * _appSettings.ProfitRatio,
                                                                previousPrices.Max() * _appSettings.ProfitRatio,
                                                            }
                                                            .Max();

                                                var alltOrderPrices = activeExchangeOrdersForThisPair.Select(o => o.Price);

                                                orderDB.Price = _traderHelperService.CalculateNewOrderPrice(Models.Exchange.OrderSideEnum.Sell, potentialNewPrice, alltOrderPrices);

                                                var newOrder = await PrepareToPlaceOrderAsync(exchangeService, pair, orderDB, activeDbOrder, Models.Exchange.OrderSideEnum.Sell, exchangeType, cancellationToken);

                                                cacheIsUpToDate = false;

                                                if (newOrder != null)
                                                {
                                                    activeExchangeOrdersCount++;
                                                    activeDbOrdersForThisPairCount++;
                                                    activeExchangeOrdersIsUpToDate = false;
                                                }
                                            }
                                            else
                                            {
                                                await _loggerService.CreateLogAsync(new LogDto(LogType.Warning, "ExecuteAsync (currentOrderHistory.AmountOriginal == 0)", new DetailedMessage { Text = string.Empty }));
                                            }
                                        }
                                        else
                                        {
                                            await _loggerService.CreateLogAsync(new LogDto(LogType.Warning, "ExecuteAsync (currentOrderHistory.PriceAverage == null)", new DetailedMessage { Text = string.Empty }));
                                        }
                                    }
                                    else
                                    {
                                        // It happened only once, while playing manually in the Exchange website.
                                        // We do not need to take any actions here.
                                        await _loggerService.CreateLogAsync(new LogDto(LogType.Warning, "ExecuteAsync (currentExchangeOrderHistory.Status is neither Canceled nor Executed)", new DetailedMessage { Text = string.Empty }));
                                    }
                                }

                                sqlContext.Orders.Update(activeDbOrder);

                                await sqlContext.SaveChangesAsync(cancellationToken);
                            }
                            else
                            {
                                // This case happens when the current order is still active and no need for action.
                            }
                        }

                        while (pair.MaxOrderLevelCount > activeDbOrdersForThisPairCount && pair.MaxOrderLevelCount > activeExchangeOrdersCount)
                        {
                            if (!activeExchangeOrdersIsUpToDate)
                            {
                                await FillPricesQueueAsync(exchangeService, pricesQueue, pairs, cancellationToken);

                                previousPrices = GetPairPriceFromQueue(pricesQueue, pair.Name);

                                activeExchangeOrders = await exchangeService.GetActiveOrdersAsync(cancellationToken);
                                activeExchangeOrdersForThisPair = activeExchangeOrders.Where(o => o.Symbol.ToLower() == $"t{pair.Name}".ToLower() && Math.Abs(o.AmountOriginal) == pair.OrderAmount).ToList();

                                activeExchangeOrdersCount = activeExchangeOrdersForThisPair.Count(oB => Math.Abs(oB.AmountOriginal) == pair.OrderAmount || activeDbOrdersForThisPair.Any(o => o.OrderId == oB.Id));

                                activeExchangeOrdersIsUpToDate = true;
                            }

                            var orderDB = new Order
                            {
                                Exchange = exchangeType,
                                CurrentMarketPrice = previousPrices.Last(),
                                ProfitRatio = _appSettings.ProfitRatio,
                            };

                            var potentialNewSellPrice = previousPrices.Max() * (decimal)orderDB.ProfitRatio;
                            var alltOrderPrices = activeExchangeOrdersForThisPair.Select(o => o.Price);

                            orderDB.Price = _traderHelperService.CalculateNewOrderPrice(Models.Exchange.OrderSideEnum.Sell, potentialNewSellPrice, alltOrderPrices);

                            var newOrder = await PrepareToPlaceOrderAsync(exchangeService, pair, orderDB, null, Models.Exchange.OrderSideEnum.Sell, exchangeType, cancellationToken);

                            cacheIsUpToDate = false;

                            if (newOrder == null)
                            {
                                var potentialNewBuyPrice = new List<decimal>()
                                                {
                                                    previousPrices.Min() / (decimal)orderDB.ProfitRatio,
                                                }
                                                .Min();

                                orderDB.Price = _traderHelperService.CalculateNewOrderPrice(Models.Exchange.OrderSideEnum.Buy, potentialNewBuyPrice, alltOrderPrices);

                                var newOrderBuy = await PrepareToPlaceOrderAsync(exchangeService, pair, orderDB, null, Models.Exchange.OrderSideEnum.Buy, exchangeType, cancellationToken);

                                cacheIsUpToDate = false;

                                if (newOrderBuy == null)
                                {
                                    var log = new LogDto(
                                        LogType.Error,
                                        $"{exchangeType} ExecuteAsync",
                                        new DetailedMessage { Text = $"Error while placing initial Sell/Buy ({pair.Name}) order." });

                                    await _loggerService.CreateLogAsync(log);

                                    // This 'break' escapes the loop and fixes the infinite loop in case of an error in the specific pair.
                                    break;
                                }
                                else
                                {
                                    activeExchangeOrdersCount++;
                                    activeExchangeOrdersIsUpToDate = false;
                                }
                            }
                            else
                            {
                                activeExchangeOrdersCount++;
                                activeExchangeOrdersIsUpToDate = false;
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

                        await logger.CreateLogAsync(new LogDto(LogType.Error, $"{exchangeType} ExecuteAsync (Main Try)", new DetailedMessage { Text = ex.Message }));
                    }
                    else
                    {
                        // TODO: Add log about this, if is needed.
                    }
                }

                stopWatch.Stop();

                await Helpers.TimeHelper.AddDelayBeforeNextIteration(_appSettings.TimeBetweenIterationInSeconds, stopWatch.ElapsedMilliseconds, cancellationToken);
            }
        }

        private IEnumerable<Domain.Pair> GetPairsFromDB(string cacheKeyName, MemoryCache memoryCache, SqlContext sqlContext, Exchange exchangeType)
        {
            if (memoryCache.Contains(cacheKeyName))
            {
                return memoryCache[cacheKeyName] as IEnumerable<Domain.Pair>;
            }
            else
            {
                var pairs = sqlContext.Pairs
                    .Include(p => p.PairHistory)
                    .Where(p => p.IsActive && p.Exchange == exchangeType)
                    .ToList();

                memoryCache.Set(cacheKeyName, pairs, new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(3),
                });

                return pairs;
            }
        }

        private IEnumerable<Order> GetActiveDbOrdersFromDB(ref bool cacheIsUpToDate, string cacheKeyName, MemoryCache memoryCache, SqlContext sqlContext, Exchange exchangeType)
        {
            if (_appSettings.CacheExpirationMultiplier > 0 && cacheIsUpToDate && memoryCache.Contains(cacheKeyName))
            {
                return memoryCache[cacheKeyName] as IEnumerable<Order>;
            }
            else
            {
                var dbObject = sqlContext.Orders
                    .Include(o => o.PreviousOrder)
                    .Where(o => o.Exchange == exchangeType && o.Status == Models.Exchange.OrderStatusEnum.Active.ToString())
                    .ToList();

                memoryCache.Set(cacheKeyName, dbObject, new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(_appSettings.CacheExpirationMultiplier * _appSettings.TimeBetweenIterationInSeconds),
                });

                cacheIsUpToDate = true;

                return dbObject;
            }
        }

        private async Task SendInitNotification(IEnumerable<string> listOfActiveExchanges, string updatePairsSummary)
        {
            var subMsg = listOfActiveExchanges.Count() > 1 ?
                $"s {string.Join(", ", listOfActiveExchanges)}" :
                $" {listOfActiveExchanges.Single()}";

            var msg = $"The AutoTrader service was started.   Trading on the cryptocurrency exchange{subMsg} is now activated.";

            var initMessage = new LogDto(LogType.Info, "TriggerAsync", new DetailedMessage { Text = msg + "   " + updatePairsSummary }, @event: "Starting");

            await _loggerService.CreateLogAsync(initMessage);
        }

        private async Task<Order> PrepareToPlaceOrderAsync(
                                IExchangeService exchangeService,
                                Domain.Pair pair,
                                Order orderDB,
                                Order activeDbOrder,
                                Models.Exchange.OrderSideEnum orderSide,
                                Exchange exchangeType,
                                CancellationToken cancellationToken)
        {
            var subLogMessage = string.Empty;

            if (activeDbOrder == null)
            {
                subLogMessage = "init";
                activeDbOrder = new Order();
            }

            orderDB.PreviousOrderProfitRatioToCurrentPrice =
                CalculatePreviousOrderProfitRatioToCurrentPrice(activeDbOrder, (decimal)orderDB.CurrentMarketPrice, orderSide);

            var newOrder = await exchangeService.PlaceOrderAsync(orderDB, pair, orderSide, subLogMessage, cancellationToken);

            if (newOrder != null)
            {
                var pairPairHistory = pair.PairHistory;

                switch (orderSide)
                {
                    case Models.Exchange.OrderSideEnum.Sell:
                        pairPairHistory.ExecutedBuyOrderCount += 1;
                        break;

                    case Models.Exchange.OrderSideEnum.Buy:
                        pairPairHistory.ExecutedSellOrderCount += 1;
                        break;

                    default:
                        break;
                }

                var logMsg = new LogDto(
                                    LogType.Info,
                                    $"{exchangeType} PrepareToPlaceOrderAsync({orderSide})",
                                    new DetailedMessage { Text = $"{pair.Name} {subLogMessage} {orderSide} order was placed.", NewOrder = newOrder },
                                    LogSourceType.PlaceOrder,
                                    $"{exchangeType} {pair.Name} {subLogMessage} {orderSide} order was placed.");

                await _loggerService.CreateLogAsync(logMsg);
            }
            else
            {
                // This case happens when in exchangeService.PlaceOrderAsync() appear Exception.
                // No need for additional logging.
            }

            return newOrder;
        }

        private decimal? CalculatePreviousOrderProfitRatioToCurrentPrice(Order activeDbOrder, decimal lastPrice, Models.Exchange.OrderSideEnum orderSide)
        {
            if (activeDbOrder.PreviousOrder != null)
            {
                if (activeDbOrder.CurrentMarketPrice.HasValue)
                {
                    var prices = new List<decimal>()
                        {
                            lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price,
                            lastPrice / (decimal)activeDbOrder.CurrentMarketPrice,
                        };

                    switch (orderSide)
                    {
                        case Models.Exchange.OrderSideEnum.Sell:
                            return prices.Max();

                        case Models.Exchange.OrderSideEnum.Buy:
                            return prices.Min();

                        default:
                            throw new Exception($"Unexpected value for OrderSideEnum '{orderSide}'.");
                    }
                }
                else
                {
                    return lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price;
                }
            }
            else
            {
                // This case happens when having no previous related order.
                return null;
            }
        }
    }
}
