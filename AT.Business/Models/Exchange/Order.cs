using System;

namespace AT.Business.Models.Exchange
{
    public class Order
    {
        public long Id { get; set; }

        public DateTime TimestampUpdated { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountOriginal { get; set; }

        public decimal Price { get; set; }

        // TODO : Why this is needed?
        public decimal? PriceAverage { get; set; }

        public string Symbol { get; set; }

        public OrderStatusEnum Status { get; set; }
    }
}