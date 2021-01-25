using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AT.Domain
{
    /// <summary>Order entity class.</summary>
    public class Order
    {
        private ILazyLoader LazyLoader { get; set; }

        private Order _previousOrder;

        /// <summary>Gets or sets the identifier for the order entity.</summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }

        /// <summary>Gets or sets the identifier for the Bitfinex order.</summary>
        /// <value>The order identifier.</value>
        public long OrderId { get; set; }

        /// <summary>Gets or sets the identifier for the Bitfinex client order. .</summary>
        /// <value>The client order identifier.</value>
        public long? ClientOrderId { get; set; }

        /// <summary>Gets or sets the create date.</summary>
        /// <value>The create date.</value>
        public DateTime CreateDate { get; set; }

        /// <summary>Gets or sets the executed date.</summary>
        /// <value>The executed date.</value>
        public DateTime? ExecutedDate { get; set; }

        /// <summary>Gets or sets the order level.</summary>
        /// <value>The order level.</value>
        public int OrderLevel { get; set; }

        /// <summary>Gets or sets the side.</summary>
        /// <value>The side.</value>
        public string Side { get; set; }

        /// <summary>Gets or sets the amount.</summary>
        /// <value>The amount.</value>
        public decimal Amount { get; set; }

        /// <summary>Gets or sets the amount original.</summary>
        /// <value>The amount original.</value>
        public decimal AmountOriginal { get; set; }

        /// <summary>Gets or sets the price.</summary>
        /// <value>The price.</value>
        public decimal Price { get; set; }

        /// <summary>Gets or sets the price average.</summary>
        /// <value>The price average.</value>
        public decimal? PriceAverage { get; set; }

        /// <summary>Gets or sets the profit ratio.</summary>
        /// <value>The profit ratio.</value>
        public decimal? ProfitRatio { get; set; }

        /// <summary>Gets or sets the previous order executed price.</summary>
        /// <value>The previous order executed price.</value>
        public decimal? PreviousOrderExecutedPrice { get; set; }

        /// <summary>Gets or sets the current market price.</summary>
        /// <value>The current market price.</value>
        public decimal? CurrentMarketPrice { get; set; }

        /// <summary>Gets or sets the previous order profit ratio.</summary>
        /// <value>The previous order profit ratio.</value>
        public decimal? PreviousOrderProfitRatio { get; set; }

        /// <summary>Gets or sets the previous order profit ratio to current price.</summary>
        /// <value>The previous order profit ratio to current price.</value>
        public decimal? PreviousOrderProfitRatioToCurrentPrice { get; set; }

        /// <summary>Gets or sets the status.</summary>
        /// <value>The status.</value>
        public string Status { get; set; }

        /// <summary>Gets or sets the symbol.</summary>
        /// <value>The symbol.</value>
        public string Symbol { get; set; }

        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public string Type { get; set; }

        /// <summary>Gets or sets the source.</summary>
        /// <value>The source.</value>
        public string Source { get; set; }

        /// <summary>Gets or sets the previous order identifier.</summary>
        /// <value>The previous order identifier.</value>
        public long? PreviousOrderId { get; set; }

        /// <summary>Gets or sets the previous order.</summary>
        /// <value>The previous order.</value>
        public virtual Order PreviousOrder
        {
            get => LazyLoader.Load(this, ref _previousOrder);
            set => _previousOrder = value;
        }
    }
}