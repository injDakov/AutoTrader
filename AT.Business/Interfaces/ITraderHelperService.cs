using System.Collections.Generic;
using AT.Business.Models.Exchange;

namespace AT.Business.Interfaces
{
    public interface ITraderHelperService
    {
        decimal CalculateNewOrderPrice(OrderSideEnum orderSide, decimal potentialNewPrice, IEnumerable<decimal> alltOrderPrices);
    }
}