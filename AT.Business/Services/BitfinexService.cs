using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Business.Models;
using AT.Business.Models.Dto;
using AT.Domain;
using AT.Domain.Enums;
using AutoMapper;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Enums;
using Bitfinex.Net.Objects;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    public class BitfinexService : IExchangeService
    {
        private readonly BitfinexClient _bitfinexClient;

        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly IDbService _dbService;
        private readonly IMapper _mapper;

        private readonly Models.AppSettings.AppSettings _appSettings;

        public BitfinexService(IConfiguration configuration, ILoggerService loggerService, IDbService dbService, IMapper mapper)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _dbService = dbService;
            _mapper = mapper;

            _appSettings = _configuration.GetSection("AppSettings").Get<Models.AppSettings.AppSettings>();

            BitfinexClient.SetDefaultOptions(new BitfinexClientOptions
            {
                ApiCredentials =
                      new ApiCredentials(
                          _appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "bitfinex").Client.Key,
                          _appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "bitfinex").Client.Secret),
            });

            _bitfinexClient = new BitfinexClient();
        }

        public async Task<IEnumerable<Models.Exchange.Order>> GetActiveOrdersAsync(CancellationToken cancellationToken)
        {
            int activeOrdersRequestMaxTry = 5;

            do
            {
                var activeOrdersRequest = await _bitfinexClient.SpotApi.Trading.GetOpenOrdersAsync(cancellationToken);

                if (!activeOrdersRequest.Success)
                {
                    activeOrdersRequestMaxTry -= 1;

                    if (activeOrdersRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "GetBitfinexActiveOrdersAsync", new DetailedMessage { Text = activeOrdersRequest.Error.Message }));

                        throw new Exception("processed");
                    }

                    await Helpers.TimeHelper.AddDelayAfterOperation(20, cancellationToken);
                }
                else
                {
                    return _mapper.Map<IEnumerable<Models.Exchange.Order>>(activeOrdersRequest.Data);
                }
            }
            while (true);
        }

        public async Task<IEnumerable<Models.Exchange.SymbolOverview>> GetPricesAsync(IEnumerable<Pair> pairs, CancellationToken cancellationToken)
        {
            int pricesRequestMaxTry = 5;

            do
            {
                var listOfSymbols = new List<string>() { "fUSD" };
                listOfSymbols.AddRange(pairs.Select(p => $"t{p.Name}"));

                var pricesRequest = await _bitfinexClient.SpotApi.ExchangeData.GetTickersAsync(listOfSymbols, cancellationToken);

                if (!pricesRequest.Success)
                {
                    pricesRequestMaxTry -= 1;

                    if (pricesRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "GetBitfinexPricesAsync", new DetailedMessage { Text = pricesRequest.Error.Message }));

                        throw new Exception("processed");
                    }

                    await Helpers.TimeHelper.AddDelayAfterOperation(20, cancellationToken);
                }
                else
                {
                    return _mapper.Map<IEnumerable<Models.Exchange.SymbolOverview>>(pricesRequest.Data);
                }
            }
            while (true);
        }

        public async Task<Models.Exchange.Order> GetOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken cancellationToken)
        {
            int ordersHistoryRequestMaxTry = 5;
            int ordersHistoryNullMaxTry = 5;

            do
            {
                var ordersHistoryRequest = await _bitfinexClient.SpotApi.Trading.GetClosedOrdersAsync(symbol: $"t{pair.Name}", ct: cancellationToken);

                if (!ordersHistoryRequest.Success)
                {
                    ordersHistoryRequestMaxTry -= 1;

                    if (ordersHistoryRequestMaxTry == 0)
                    {
                        await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "GetBitfinexOrderHistoryAsync", new DetailedMessage { Text = ordersHistoryRequest.Error.Message }));

                        throw new Exception("processed");
                    }

                    await Helpers.TimeHelper.AddDelayAfterOperation(20, cancellationToken);
                }
                else
                {
                    if (!ordersHistoryRequest.Data.Any(o => o.Id == activeOrderId))
                    {
                        ordersHistoryNullMaxTry -= 1;

                        if (ordersHistoryNullMaxTry == 0)
                        {
                            await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "GetBitfinexOrderHistoryAsync", new DetailedMessage { Text = "Null" }));

                            return null;
                        }

                        await Helpers.TimeHelper.AddDelayAfterOperation(20, cancellationToken);
                    }
                    else
                    {
                        return _mapper.Map<Models.Exchange.Order>(ordersHistoryRequest.Data.FirstOrDefault(o => o.Id == activeOrderId));
                    }
                }
            }
            while (true);
        }

        public async Task<Order> PlaceOrderAsync(Order orderDB, Pair pair, Models.Exchange.OrderSideEnum orderSide, string subLogMessage, CancellationToken cancellationToken)
        {
            int placeOrderRequestMaxTry = 5;

            do
            {
                var placeOrderRequest =
                    await _bitfinexClient.SpotApi.Trading.PlaceOrderAsync(
                                            symbol: $"t{pair.Name}",
                                            side: _mapper.Map<OrderSide>(orderSide),
                                            type: OrderType.ExchangeLimit,
                                            quantity: pair.OrderAmount,
                                            price: orderDB.Price,
                                            ct: cancellationToken);

                if (!placeOrderRequest.Success)
                {
                    placeOrderRequestMaxTry -= 1;

                    if (placeOrderRequestMaxTry == 0 || placeOrderRequest.Error.Message.Contains("not enough exchange balance for"))
                    {
                        await _loggerService.CreateLogAsync(new LogDto(LogType.Error, "PlaceOrderAsync", new DetailedMessage { Text = $"{placeOrderRequest.Error.Message} {subLogMessage}" }));

                        return null;
                    }

                    await Helpers.TimeHelper.AddDelayAfterOperation(20, cancellationToken);
                }
                else
                {
                    var placedOrder = _mapper.Map<PlacedOrder>(placeOrderRequest.Data.Data);
                    placedOrder.Side = orderSide.ToString();

                    return await _dbService.AddDbOrderAsync(placedOrder, orderDB);
                }
            }
            while (true);
        }
    }
}