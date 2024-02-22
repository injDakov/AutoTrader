using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AT.Domain;

namespace AT.Business.Interfaces
{
    public interface IExchangeService
    {
        Task<IEnumerable<Models.Exchange.Order>> GetActiveOrdersAsync(CancellationToken cancellationToken);

        Task<IEnumerable<Models.Exchange.SymbolOverview>> GetPricesAsync(IEnumerable<Pair> pairs, CancellationToken cancellationToken);

        Task<Models.Exchange.Order> GetOrderHistoryAsync(Pair pair, long activeOrderId, CancellationToken cancellationToken);

        Task<Order> PlaceOrderAsync(Order orderDB, Pair pair, Models.Exchange.OrderSideEnum orderSide, string subLogMessage, CancellationToken cancellationToken);
    }
}