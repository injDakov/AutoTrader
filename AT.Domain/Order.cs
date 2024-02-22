using System;
using AT.Domain.Enums;

namespace AT.Domain
{
    public class Order
    {
        public long Id { get; set; }

        public Exchange Exchange { get; set; }

        public long OrderId { get; set; }

        public long? ClientOrderId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? ExecutedDate { get; set; }

        public string Side { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountOriginal { get; set; }

        public decimal Price { get; set; }

        public decimal? PriceAverage { get; set; }

        public decimal? ProfitRatio { get; set; }

        public decimal? PreviousOrderExecutedPrice { get; set; }

        public decimal? CurrentMarketPrice { get; set; }

        public decimal? PreviousOrderProfitRatio { get; set; }

        public decimal? PreviousOrderProfitRatioToCurrentPrice { get; set; }

        public string Status { get; set; }

        public string Symbol { get; set; }

        public string Type { get; set; }

        public long? PreviousOrderId { get; set; }

        public virtual Order PreviousOrder { get; set; }
    }
}