using System.Threading;
using System.Threading.Tasks;
using AT.Business.Models;
using AT.Domain;

namespace AT.Business.Interfaces
{
    public interface IDbService
    {
        Task<string> UpdatePairsConfigurationAsync(CancellationToken cancellationToken);

        Task<Order> AddDbOrderAsync(PlacedOrder placedOrder, Order dbOrder);
    }
}