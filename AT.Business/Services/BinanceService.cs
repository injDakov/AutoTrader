using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AT.Business.Interfaces;
using AT.Domain;
using AutoMapper;
using Binance.Net.Clients;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    public class BinanceService : IExchangeService
    {
        private readonly BinanceClient _binanceClient;

        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly IDbService _dbService;
        private readonly IMapper _mapper;

        private readonly Models.AppSettings.AppSettings _appSettings;

        public BinanceService(IConfiguration configuration, ILoggerService loggerService, IDbService dbService, IMapper mapper)
        {
            _configuration = configuration;
            _loggerService = loggerService;
            _dbService = dbService;
            _mapper = mapper;

            _appSettings = _configuration.GetSection("AppSettings").Get<Models.AppSettings.AppSettings>();

            //BinanceClient.SetDefaultOptions(new BinanceClientOptions
            //{
            //    ApiCredentials =
            //          new ApiCredentials(
            //              _appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "binance").Client.Key,
            //              _appSettings.ExchangesSettings.Single(s => s.Name.ToLower() == "binance").Client.Secret),
            //});

            _binanceClient = new BinanceClient();
        }

        public async Task<IEnumerable<Models.Exchange.Order>> GetActiveOrdersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Binance's GetActiveOrdersAsync method is still not implemented.");
        }

        public async Task<IEnumerable<Models.Exchange.SymbolOverview>> GetPricesAsync(IEnumerable<Pair> pairs, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Binance's GetPricesAsync method is still not implemented.");
        }

        public async Task<Models.Exchange.Order> GetOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Binance's GetOrderHistoryAsync method is still not implemented.");
        }

        public async Task<Order> PlaceOrderAsync(Order orderDB, Pair pair, Models.Exchange.OrderSideEnum orderSide, string subLogMessage, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Binance's PlaceOrderAsync method is still not implemented.");
        }
    }
}