using System.Threading.Tasks;
using AT.Domain;
using Bitfinex.Net.Objects.RestV1Objects;

namespace AT.Business.Interfaces
{
    /// <summary>IDbOrderService Interface.</summary>
    public interface IDbOrderService
    {
        /// <summary>Updates the database pairs.</summary>
        /// <returns>Task.</returns>
        Task UpdateDbPairs();

        /// <summary>Adds the database order.</summary>
        /// <param name="bitfinexPlacedOrder">The bitfinex placed order.</param>
        /// <param name="dbOrder">The database order.</param>
        /// <returns>Task of Order.</returns>
        Task<Order> AddDbOrder(BitfinexPlacedOrder bitfinexPlacedOrder, Order dbOrder);
    }
}