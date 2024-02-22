using System.Threading.Tasks;
using AT.Business.Models.Dto;

namespace AT.Business.Interfaces
{
    public interface ILoggerService
    {
        Task CreateLogAsync(LogDto log);
    }
}