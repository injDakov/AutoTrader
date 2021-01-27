using System.Threading.Tasks;
using AT.Domain;

namespace AT.Business.Interfaces
{
    /// <summary>ILoggerService interface.</summary>
    public interface ILoggerService
    {
        /// <summary>Creates the log.</summary>
        /// <param name="log">The log.</param>
        /// <returns>Task.</returns>
        Task CreateLog(Log log);
    }
}