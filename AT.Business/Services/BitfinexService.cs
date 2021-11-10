using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Domain;
using AT.Domain.Enums;
using Bitfinex.Net;
using Bitfinex.Net.Objects;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    /// <summary>BitfinexService class.</summary>
    public class BitfinexService : IBitfinexService
    {
        private readonly string _assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();

        private readonly BitfinexClient _bitfinexClient;

        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly IDbOrderService _dbOrderService;

        private readonly Models.AppSettings.AppSettings _appSettings;

        /// <summary>Initializes a new instance of the <see cref="BitfinexService" /> class.</summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="loggerService">The logger service.</param>
        /// <param name="dbOrderService">The database order service.</param>
        public BitfinexService(IConfiguration configuration, ILoggerService loggerService, IDbOrderService dbOrderService)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _dbOrderService = dbOrderService;

            _appSettings = _configuration.GetSection("AppSettings").Get<Models.AppSettings.AppSettings>();

            BitfinexClient.SetDefaultOptions(new BitfinexClientOptions
            {
                ApiCredentials =
                      new ApiCredentials(
                          _appSettings.BitfinexClient.Key,
                          _appSettings.BitfinexClient.Secret),
            });

            _bitfinexClient = new BitfinexClient();
        }

        /// <summary>Gets the Bitfinex active orders asynchronous.</summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of IEnumerable of BitfinexOrder.</returns>
        /// <exception cref="Exception">processed.</exception>
        public async Task<IEnumerable<BitfinexOrder>> GetBitfinexActiveOrdersAsync(CancellationToken stoppingToken)
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
                        await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexActiveOrdersAsync()", activeOrdersRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await AddDelayAfterOperation(stoppingToken);
                }
                else
                {
                    return activeOrdersRequest.Data;
                }
            }
            while (true);
        }

        /// <summary>Gets the Bitfinex prices asynchronous.</summary>
        /// <param name="pairs">The pairs.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of IEnumerable of BitfinexSymbolOverview.</returns>
        /// <exception cref="Exception">processed.</exception>
        public async Task<IEnumerable<BitfinexSymbolOverview>> GetBitfinexPricesAsync(IEnumerable<Pair> pairs, CancellationToken stoppingToken)
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
                        await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexPricesAsync()", pricesRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await AddDelayAfterOperation(stoppingToken);
                }
                else
                {
                    return pricesRequest.Data;
                }
            }
            while (true);
        }

        /// <summary>Gets the Bitfinex order history asynchronous.</summary>
        /// <param name="pair">The pair.</param>
        /// <param name="activeOrderId">The active order identifier.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of BitfinexOrder.</returns>
        /// <exception cref="Exception">processed.</exception>
        public async Task<BitfinexOrder> GetBitfinexOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken stoppingToken)
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
                        await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexOrderHistoryAsync()", ordersHistoryRequest.Error.Message));

                        throw new Exception("processed");
                    }

                    await AddDelayAfterOperation(stoppingToken);
                }
                else
                {
                    if (!ordersHistoryRequest.Data.Any(o => o.Id == activeOrderId))
                    {
                        ordersHistoryNullMaxTry -= 1;

                        if (ordersHistoryNullMaxTry == 0)
                        {
                            await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, GetBitfinexOrderHistoryAsync()", "Null"));

                            return null;
                        }

                        await AddDelayAfterOperation(stoppingToken);
                    }
                    else
                    {
                        return ordersHistoryRequest.Data.FirstOrDefault(o => o.Id == activeOrderId);
                    }
                }
            }
            while (true);
        }

        /// <summary>Places the new order asynchronous.</summary>
        /// <param name="orderDB">The order database.</param>
        /// <param name="pair">The pair.</param>
        /// <param name="orderSide">The order side.</param>
        /// <param name="orderPrice">The order price.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of Order.</returns>
        public async Task<Order> PlaceNewOrderAsync(Order orderDB, Pair pair, OrderSide orderSide, decimal orderPrice, CancellationToken stoppingToken)
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
                        await _loggerService.CreateLogAsync(new Log(LogType.Error, $"AutoTrader v{_assemblyVersion}, PlaceNewOrderAsync()", placeOrderRequest.Error.Message));

                        return null;
                    }

                    await AddDelayAfterOperation(stoppingToken);
                }
                else
                {
                    return await _dbOrderService.AddDbOrderAsync(placeOrderRequest.Data, orderDB);
                }
            }
            while (true);
        }

        private async Task AddDelayAfterOperation(CancellationToken stoppingToken)
        {
            await Task.Delay(1000 * 20 * 1, stoppingToken);
        }
    }
}