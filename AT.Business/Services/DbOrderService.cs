using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Common.Extensions;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using Bitfinex.Net.Objects;
using Bitfinex.Net.Objects.RestV1Objects;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    /// <summary>DbOrderService class.</summary>
    public class DbOrderService : IDbOrderService
    {
        private readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly SqlContext _sqlContext;

        private readonly Models.AppSettings.AppSettings _appSettings;

        /// <summary>Initializes a new instance of the <see cref="DbOrderService" /> class.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="loggerService">The logger service.</param>
        /// <param name="sqlContext">The SQL context.</param>
        public DbOrderService(IConfiguration configuration, ILoggerService loggerService, SqlContext sqlContext)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _sqlContext = sqlContext;

            _appSettings = _configuration.GetSection("AppSettings").Get<Models.AppSettings.AppSettings>();
        }

        /// <summary>Updates the database pairs.</summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <exception cref="Exception">Exception.</exception>
        /// <returns>Task.</returns>
        public async Task UpdateDbPairsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configPairs = _appSettings.Pairs;
                var sellLevels = _appSettings.ProfitPercents.SellLevels;
                var buyLevels = _appSettings.ProfitPercents.BuyLevels;

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

                if (configPairs.Any(p => p.MaxOrderLevel > sellLevels.Length || p.MaxOrderLevel > buyLevels.Length))
                {
                    string msg = $"The pair {configPairs.FirstOrDefault(p => p.MaxOrderLevel > sellLevels.Length || p.MaxOrderLevel > buyLevels.Length).Name} has value for MaxOrderLevel that was bigger than BuyLevels/SellLevels. Please check and restart the service!";

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

                            if (!pair.IsActive)
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
                await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, UpdateDbPairs()", ex.Message));

                await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, UpdateDbPairs()", "The service has stopped incidentally."));

                throw;
            }
        }

        /// <summary>Adds the database order.</summary>
        /// <param name="bitfinexPlacedOrder">The Bitfinex placed order.</param>
        /// <param name="dbOrder">The database order.</param>
        /// <returns>Task of Order.</returns>
        /// <exception cref="Exception">processed.</exception>
        public async Task<Order> AddDbOrderAsync(BitfinexPlacedOrder bitfinexPlacedOrder, Order dbOrder)
        {
            try
            {
                var newOrder = new Order
                {
                    OrderId = bitfinexPlacedOrder.Id.ConvertToLong(),
                    ClientOrderId = bitfinexPlacedOrder.ClientOrderId,

                    CreateDate = bitfinexPlacedOrder.Timestamp,

                    OrderLevel = dbOrder.OrderLevel,
                    Side = bitfinexPlacedOrder.Side.ToString(),

                    Amount = bitfinexPlacedOrder.RemainingAmount,
                    AmountOriginal = bitfinexPlacedOrder.OriginalAmount,

                    Price = bitfinexPlacedOrder.Price,
                    PriceAverage = bitfinexPlacedOrder.AverageExecutionPrice,
                    ProfitRatio = dbOrder.ProfitRatio,

                    PreviousOrderExecutedPrice = dbOrder?.PreviousOrderExecutedPrice,
                    CurrentMarketPrice = dbOrder.CurrentMarketPrice,

                    PreviousOrderProfitRatio = dbOrder.PreviousOrderProfitRatio,
                    PreviousOrderProfitRatioToCurrentPrice = dbOrder.PreviousOrderProfitRatioToCurrentPrice,

                    Status = OrderStatus.Active.ToString(),

                    Symbol = bitfinexPlacedOrder.Symbol,
                    Type = bitfinexPlacedOrder.Type.ToString(),
                    Source = bitfinexPlacedOrder.Source,

                    PreviousOrderId = dbOrder.Id != 0 ? dbOrder.Id : (long?)null,
                };

                await _sqlContext.Orders.AddAsync(newOrder);
                await _sqlContext.SaveChangesAsync();

                return newOrder;
            }
            catch (Exception ex)
            {
                await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, AddDbOrder()", ex.Message));

                throw new Exception("processed");
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
    }
}