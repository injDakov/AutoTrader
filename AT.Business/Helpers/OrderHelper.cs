using System;
using System.Collections.Generic;
using System.Linq;
using AT.Business.Models.Exchange;

namespace AT.Business.Helpers
{
    public static class OrderHelper
    {
        public static decimal? CalculatePreviousOrderProfitRatioToCurrentPrice(Domain.Order activeDbOrder, decimal lastPrice, OrderSideEnum orderSide)
        {
            if (activeDbOrder.PreviousOrder != null)
            {
                if (activeDbOrder.CurrentMarketPrice.HasValue)
                {
                    var prices = new List<decimal>()
                        {
                            lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price,
                            lastPrice / (decimal)activeDbOrder.CurrentMarketPrice,
                        };

                    return orderSide switch
                    {
                        OrderSideEnum.Sell => prices.Max(),
                        OrderSideEnum.Buy => prices.Min(),
                        _ => throw new Exception($"Unexpected value for OrderSideEnum '{orderSide}'."),
                    };
                }
                else
                {
                    return lastPrice / (decimal)activeDbOrder.PreviousOrder?.Price;
                }
            }
            else
            {
                // This case happens when having no previous related order.
                return null;
            }
        }
    }
}
