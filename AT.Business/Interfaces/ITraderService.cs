using System.Threading;
using System.Threading.Tasks;

namespace AT.Business.Interfaces
{
    public interface ITraderService
    {
        Task TriggerAsync(CancellationToken cancellationToken);
    }
}