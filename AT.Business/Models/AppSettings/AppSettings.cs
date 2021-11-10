using System.Collections.Generic;

namespace AT.Business.Models.AppSettings
{
    /// <summary>AppSettings class.</summary>
    public class AppSettings
    {
        /// <summary>Gets or sets the SMTP server.</summary>
        /// <value>The SMTP server.</value>
        public SmtpServer SmtpServer { get; set; }

        /// <summary>Gets or sets the Bitfinex client.</summary>
        /// <value>The Bitfinex client.</value>
        public BitfinexClient BitfinexClient { get; set; }

        /// <summary>Gets or sets the health check interval in hours.</summary>
        /// <value>The health check interval in hours.</value>
        public int HealthCheckIntervalInHours { get; set; }

        /// <summary>Gets or sets the size of the prices queue.</summary>
        /// <value>The size of the prices queue.</value>
        public int PricesQueueSize { get; set; }

        /// <summary>Gets or sets the profit percents.</summary>
        /// <value>The profit percents.</value>
        public ProfitPercents ProfitPercents { get; set; }

        /// <summary>Gets or sets the pairs.</summary>
        /// <value>The pairs.</value>
        public IEnumerable<Pair> Pairs { get; set; }
    }
}