using System.Threading;
using System.Threading.Tasks;
using AT.Domain;
using Bitfinex.Net.Objects.RestV1Objects;

namespace AT.Business.Interfaces
{
    /// <summary>IDbOrderService interface.</summary>
    public interface IDbOrderService
    {
        /// <summary>Updates the database pairs.</summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>Task.</returns>
        Task UpdateDbPairsAsync(CancellationToken cancellationToken);

        /// <summary>Adds the database order.</summary>
        /// <param name="bitfinexPlacedOrder">The Bitfinex placed order.</param>
        /// <param name="dbOrder">The database order.</param>
        /// <returns>Task of Order.</returns>
        Task<Order> AddDbOrderAsync(BitfinexPlacedOrder bitfinexPlacedOrder, Order dbOrder);
    }
}