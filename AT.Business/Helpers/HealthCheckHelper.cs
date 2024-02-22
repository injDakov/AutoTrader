using System;
using System.Collections.Generic;
using System.Linq;

namespace AT.Business.Helpers
{
    public static class HealthCheckHelper
    {
        public static bool IsItTime(DateTime dateTime, int healthCheckIntervalInHours, IEnumerable<int> healthCheckHours)
        {
            var intervalsCheck =
                healthCheckIntervalInHours != 0 &&
                dateTime.Hour != 0 &&
                dateTime.Hour % healthCheckIntervalInHours == 0 &&
                dateTime.Minute == 0;

            var hoursCheck =
                healthCheckHours != null &&
                healthCheckHours.Contains(dateTime.Hour) &&
                dateTime.Minute == 0;

            return intervalsCheck || hoursCheck;
        }
    }
}