using System;
using System.Collections.Generic;
using System.Linq;

namespace AT.Business.Models.AppSettings
{
    public class AppSettings
    {
        private int _timeBetweenIterationInSeconds;
        private int _cacheExpirationMultiplier;
        private int _healthCheckIntervalInHours;
        private IEnumerable<int> _healthCheckHours = Enumerable.Empty<int>();
        private int _pricesQueueSize;
        private decimal _minRatioToNearestOrder;
        private decimal _profitRatio;

        public SmtpServer SmtpServer { get; set; }

        public int TimeBetweenIterationInSeconds
        {
            get
            {
                return _timeBetweenIterationInSeconds;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.TimeBetweenIterationInSeconds' has to be bigger than '0', but the current value is '{value}' !");
                }

                _timeBetweenIterationInSeconds = value;
            }
        }

        public int CacheExpirationMultiplier
        {
            get
            {
                return _cacheExpirationMultiplier;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.CacheExpirationMultiplier' has to be '0' (the cache is deactivated) or bigger, but the current value is '{value}' !");
                }

                _cacheExpirationMultiplier = value;
            }
        }

        public int HealthCheckIntervalInHours
        {
            get
            {
                return _healthCheckIntervalInHours;
            }

            set
            {
                if (value < 0 || value > 23)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.HealthCheckIntervalInHours' has to be between '0' (deactivated) and '23', but the current value is '{value}' !");
                }

                _healthCheckIntervalInHours = value;
            }
        }

        public IEnumerable<int> HealthCheckHours
        {
            get
            {
                return _healthCheckHours;
            }

            set
            {
                if (value is null)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.HealthCheckHours' is not allowed to be null !");
                }

                if (value.Any(x => x < 0 || x > 23))
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.HealthCheckHours' has to contain elements with value between '0' and '23' !");
                }

                if (value.GroupBy(n => n).Any(group => group.Count() > 1))
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.HealthCheckHours' has to contain unique elements !");
                }

                _healthCheckHours = value;
            }
        }

        public int PricesQueueSize
        {
            get
            {
                return _pricesQueueSize;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.PricesQueueSize' has to be bigger than '0', but the current value is '{value}' !");
                }

                _pricesQueueSize = value;
            }
        }

        public decimal ProfitRatio
        {
            get
            {
                return _profitRatio;
            }

            set
            {
                if (value <= 1)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.ProfitRatio' has to be bigger than '1', but the current value is '{value}' !");
                }

                _profitRatio = value;
            }
        }

        public decimal MinRatioToNearestOrder
        {
            get
            {
                return _minRatioToNearestOrder;
            }

            set
            {
                if (value <= 1)
                {
                    throw new ArgumentOutOfRangeException($"The 'AppSettings.MinRatioToNearestOrder' has to be bigger than '1', but the current value is '{value}' !");
                }

                _minRatioToNearestOrder = value;
            }
        }

        public IEnumerable<ExchangeSettings> ExchangesSettings { get; set; }
    }
}