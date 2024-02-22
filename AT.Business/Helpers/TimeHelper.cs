using System.Threading;
using System.Threading.Tasks;

namespace AT.Business.Helpers
{
    public static class TimeHelper
    {
        public static async Task AddDelayBeforeNextIteration(int timeBetweenIterationInSeconds, long elapsedMilliseconds, CancellationToken cancellationToken)
        {
            int timeToNextIteration = 1000 * timeBetweenIterationInSeconds;

            if (elapsedMilliseconds > timeToNextIteration)
            {
                timeToNextIteration = 0;
            }
            else
            {
                timeToNextIteration -= (int)elapsedMilliseconds;
            }

            await Task.Delay(timeToNextIteration, cancellationToken);
        }

        public static async Task AddDelayAfterOperation(int delayInSeconds, CancellationToken cancellationToken)
        {
            await Task.Delay(1000 * delayInSeconds, cancellationToken);
        }
    }
}