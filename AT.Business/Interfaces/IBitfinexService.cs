using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AT.Domain;
using Bitfinex.Net.Objects;

namespace AT.Business.Interfaces
{
    /// <summary>IBitfinexService interface.</summary>
    public interface IBitfinexService
    {
        /// <summary>Gets the Bitfinex active orders asynchronous.</summary>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of IEnumerable of BitfinexOrder.</returns>
        Task<IEnumerable<BitfinexOrder>> GetBitfinexActiveOrdersAsync(CancellationToken stoppingToken);

        /// <summary>Gets the Bitfinex prices asynchronous.</summary>
        /// <param name="pairs">The pairs.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of IEnumerable of BitfinexSymbolOverview.</returns>
        Task<IEnumerable<BitfinexSymbolOverview>> GetBitfinexPricesAsync(IEnumerable<Pair> pairs, CancellationToken stoppingToken);

        /// <summary>Gets the Bitfinex order history asynchronous.</summary>
        /// <param name="pair">The pair.</param>
        /// <param name="activeOrderId">The active order identifier.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of BitfinexOrder.</returns>
        Task<BitfinexOrder> GetBitfinexOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken stoppingToken);

        /// <summary>Places the new order asynchronous.</summary>
        /// <param name="orderDB">The order database.</param>
        /// <param name="pair">The pair.</param>
        /// <param name="orderSide">The order side.</param>
        /// <param name="orderPrice">The order price.</param>
        /// <param name="stoppingToken">The stopping token.</param>
        /// <returns>Task of Order.</returns>
        Task<Order> PlaceNewOrderAsync(Order orderDB, Pair pair, OrderSide orderSide, decimal orderPrice, CancellationToken stoppingToken);
    }
}