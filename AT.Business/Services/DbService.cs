using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Business.Models.Dto;
using AT.Data;
using AT.Domain;
using AT.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    public class DbService : IDbService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly SqlContext _sqlContext;

        private readonly Models.AppSettings.AppSettings _appSettings;

        public DbService(IConfiguration configuration, ILoggerService loggerService, SqlContext sqlContext)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _sqlContext = sqlContext;

            _appSettings = _configuration.GetSection("AppSettings").Get<Models.AppSettings.AppSettings>();
        }

        public async Task<string> UpdatePairsConfigurationAsync(CancellationToken cancellationToken)
        {
            try
            {
                // TODO: Create test !

                var updatedPairsMsgList = new List<string>();

                var exchangesSettings = _appSettings.ExchangesSettings;

                foreach (var exchangeSettings in exchangesSettings)
                {
                    var configPairs = exchangeSettings.Pairs.ToList();

                    Helpers.ConfigHelper.CheckPairConfiguration(configPairs);

                    var exchangeEnum = (Exchange)Enum.Parse(typeof(Exchange), exchangeSettings.Name);

                    var dbPairs = _sqlContext.Pairs.Where(p => p.Exchange == exchangeEnum);

                    foreach (var dbPair in dbPairs.Where(p => p.IsActive))
                    {
                        if (!configPairs.Any(pc => pc.Name == dbPair.Name))
                        {
                            updatedPairsMsgList.Add($"Deactivated the pair for '{exchangeEnum}', '{dbPair.Name}'.{Environment.NewLine}");

                            Helpers.ConfigHelper.DeactivatePair(dbPair);
                        }
                    }

                    foreach (var pair in configPairs)
                    {
                        var dbPair = dbPairs.FirstOrDefault(p => p.Exchange == exchangeEnum && p.Name == pair.Name);

                        if (dbPair == null)
                        {
                            // Adding a new Pair from the config file to the database.

                            var dateTimeUtcNow = DateTime.UtcNow;

                            var newPair = new Pair
                            {
                                CreateDate = dateTimeUtcNow,
                                StartDate = dateTimeUtcNow,

                                Exchange = exchangeEnum,
                                Name = pair.Name,
                                OrderAmount = pair.OrderAmount,
                                MaxOrderLevelCount = pair.MaxOrderLevelCount,

                                IsActive = pair.IsActive,
                            };

                            Helpers.ConfigHelper.AddPairOrders(newPair);

                            updatedPairsMsgList.Add($"Added a new pair for '{exchangeEnum}', '{pair.Name}' with OrderAmount '{pair.OrderAmount}' and MaxOrderLevelCount '{pair.MaxOrderLevelCount}'.{Environment.NewLine}");

                            _sqlContext.Pairs.Add(newPair);
                        }
                        else
                        {
                            if (dbPair.OrderAmount != pair.OrderAmount)
                            {
                                updatedPairsMsgList.Add($"Updated the pair for '{exchangeEnum}', '{dbPair.Name}' with the new OrderAmount '{pair.OrderAmount}' (the old was '{dbPair.OrderAmount}').{Environment.NewLine}");

                                // Updating the existing DB pair with changed data from the actual config file.
                                dbPair.OrderAmount = pair.OrderAmount;
                                dbPair.LastUpdateDate = DateTime.UtcNow;
                            }

                            if (dbPair.MaxOrderLevelCount != pair.MaxOrderLevelCount)
                            {
                                updatedPairsMsgList.Add($"Updated the pair for '{exchangeEnum}', '{dbPair.Name}' with the new MaxOrderLevelCount '{pair.MaxOrderLevelCount}' (the old was '{dbPair.MaxOrderLevelCount}').{Environment.NewLine}");

                                if (pair.MaxOrderLevelCount == 0)
                                {
                                    Helpers.ConfigHelper.DeactivatePairOrders(dbPair.PairHistory);
                                }

                                if (dbPair.MaxOrderLevelCount == 0 && pair.MaxOrderLevelCount > 0)
                                {
                                    Helpers.ConfigHelper.ActivatePairOrders(dbPair.PairHistory);
                                }

                                dbPair.MaxOrderLevelCount = pair.MaxOrderLevelCount;
                                dbPair.LastUpdateDate = DateTime.UtcNow;
                            }

                            if (dbPair.IsActive != pair.IsActive)
                            {
                                dbPair.IsActive = pair.IsActive;

                                if (!pair.IsActive)
                                {
                                    updatedPairsMsgList.Add($"Deactivated the pair for '{exchangeEnum}', '{dbPair.Name}'.{Environment.NewLine}");

                                    Helpers.ConfigHelper.DeactivatePair(dbPair);
                                }
                                else
                                {
                                    updatedPairsMsgList.Add($"Activated the pair for '{exchangeEnum}', '{dbPair.Name}'.{Environment.NewLine}");

                                    Helpers.ConfigHelper.ActivatePair(dbPair);
                                }
                            }
                        }
                    }
                }

                _sqlContext.SaveChanges();

                if (updatedPairsMsgList.Count > 0)
                {
                    return $"The summary of pairs' updating: {string.Join(", ", updatedPairsMsgList)}.";
                }
                else
                {
                    return "All trading pairs from the database have been synchronized with the configuration, ensuring they are up to date. " +
                        "No further action is required at this time.";
                }
            }
            catch (Exception ex)
            {
                await _loggerService.CreateLogAsync(new LogDto(LogType.Error, $"UpdateDbPairsAsync", new DetailedMessage { Text = ex.Message + " Please check and restart the service! The service has stopped incidentally." }));

                throw;
            }
        }

        public async Task<Order> AddDbOrderAsync(PlacedOrder placedOrder, Order dbOrder)
        {
            try
            {
                var newOrder = new Order
                {
                    Exchange = dbOrder.Exchange,

                    OrderId = placedOrder.OrderId,
                    ClientOrderId = placedOrder.ClientOrderId,

                    CreateDate = placedOrder.CreateDate,

                    Side = placedOrder.Side,

                    Amount = Math.Abs(placedOrder.Amount),
                    AmountOriginal = Math.Abs(placedOrder.AmountOriginal),

                    Price = placedOrder.Price,
                    PriceAverage = placedOrder.PriceAverage,
                    ProfitRatio = dbOrder.ProfitRatio,

                    PreviousOrderExecutedPrice = dbOrder.PreviousOrderExecutedPrice,
                    CurrentMarketPrice = dbOrder.CurrentMarketPrice,

                    PreviousOrderProfitRatio = dbOrder.PreviousOrderProfitRatio,
                    PreviousOrderProfitRatioToCurrentPrice = dbOrder.PreviousOrderProfitRatioToCurrentPrice,

                    Status = Models.Exchange.OrderStatusEnum.Active.ToString(),

                    Symbol = placedOrder.Symbol,
                    Type = placedOrder.Type,

                    PreviousOrderId = dbOrder.Id != 0 ? dbOrder.Id : (long?)null,
                };

                await _sqlContext.Orders.AddAsync(newOrder);
                await _sqlContext.SaveChangesAsync();

                return newOrder;
            }
            catch (Exception ex)
            {
                await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "AddDbOrder", new DetailedMessage { Text = ex.Message }));

                throw new Exception("processed");
            }
        }
    }
}