using System;

namespace AT.Business.Models
{
    // TODO:
    public class PlacedOrder
    {
        public long OrderId { get; set; } // OrderId = placedOrder.Id.ConvertToLong(),

        public long? ClientOrderId { get; set; }

        public DateTime CreateDate { get; set; } //  CreateDate = placedOrder.Timestamp,

        public string Side { get; set; }

        public decimal Amount { get; set; } // Amount = placedOrder.RemainingAmount,

        public decimal AmountOriginal { get; set; } // AmountOriginal = placedOrder.OriginalAmount,

        public decimal Price { get; set; }

        public decimal? PriceAverage { get; set; } // PriceAverage = placedOrder.AverageExecutionPrice,

        public string Status { get; set; } // Status = OrderStatus.Active.ToString(),

        public string Symbol { get; set; }

        public string Type { get; set; }
    }
}