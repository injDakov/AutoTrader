using System.Threading.Tasks;
using AT.Domain;

namespace AT.Worker.Interfaces
{
    /// <summary>ILoggerService interface.</summary>
    public interface ILoggerService
    {
        /// <summary>Creates the log.</summary>
        /// <param name="log">The log.</param>
        /// <returns>The task.</returns>
        Task CreateLog(Log log);
    }
}