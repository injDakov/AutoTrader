using System;
using System.Collections.Generic;
using System.Linq;
using AT.Business.Interfaces;
using AT.Business.Models.AppSettings;
using AT.Business.Models.Exchange;
using Microsoft.Extensions.Configuration;

namespace AT.Business.Services
{
    public class TraderHelperService : ITraderHelperService
    {
        private readonly IConfiguration _configuration;

        private readonly AppSettings _appSettings;

        public TraderHelperService(IConfiguration configuration)
        {
            _configuration = configuration;

            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        public decimal CalculateNewOrderPrice(OrderSideEnum orderSide, decimal potentialNewPrice, IEnumerable<decimal> allOrderPrices)
        {
            try
            {
                if (allOrderPrices == null || !allOrderPrices.Any())
                {
                    throw new ArgumentException($"Unexpected value for allOrderPrices '{allOrderPrices}' inside the CalculateNewOrderPrice().");
                }

                switch (orderSide)
                {
                    case OrderSideEnum.Sell:
                        allOrderPrices = allOrderPrices.Order();

                        if (!allOrderPrices.Any(p => p > potentialNewPrice))
                        {
                            if (potentialNewPrice < allOrderPrices.Max() * _appSettings.MinRatioToNearestOrder)
                            {
                                potentialNewPrice = allOrderPrices.Max() * _appSettings.MinRatioToNearestOrder;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < allOrderPrices.Count(); i++)
                            {
                                decimal prevPrice = i > 0 ? allOrderPrices.ElementAt(i - 1) : 0;
                                decimal nextPrice = allOrderPrices.ElementAt(i);

                                if (nextPrice > potentialNewPrice)
                                {
                                    decimal priceMultiplier = 1;

                                    if (prevPrice * _appSettings.MinRatioToNearestOrder > potentialNewPrice)
                                    {
                                        priceMultiplier = prevPrice;
                                    }
                                    else
                                    {
                                        if (potentialNewPrice > nextPrice / _appSettings.MinRatioToNearestOrder)
                                        {
                                            priceMultiplier = nextPrice;
                                        }
                                        else
                                        {
                                            // There is no need of shifting the order's price.
                                            return potentialNewPrice;
                                        }
                                    }

                                    potentialNewPrice = priceMultiplier * _appSettings.MinRatioToNearestOrder;

                                    for (int j = i; j < allOrderPrices.Count(); j++)
                                    {
                                        decimal prevPrice2 = j != 0 ? allOrderPrices.ElementAt(j - 1) : 0;
                                        decimal nextPrice2 = allOrderPrices.ElementAt(j);

                                        if (prevPrice2 * _appSettings.MinRatioToNearestOrder <= potentialNewPrice && potentialNewPrice <= nextPrice2 / _appSettings.MinRatioToNearestOrder)
                                        {
                                            // There is no need of shifting the order's price.
                                            return potentialNewPrice;
                                        }
                                        else
                                        {
                                            potentialNewPrice = nextPrice2 * _appSettings.MinRatioToNearestOrder;
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    case OrderSideEnum.Buy:
                        allOrderPrices = allOrderPrices.OrderDescending();

                        if (!allOrderPrices.Any(p => p < potentialNewPrice))
                        {
                            if (potentialNewPrice > allOrderPrices.Min() / _appSettings.MinRatioToNearestOrder)
                            {
                                potentialNewPrice = allOrderPrices.Min() / _appSettings.MinRatioToNearestOrder;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < allOrderPrices.Count(); i++)
                            {
                                decimal prevPrice = allOrderPrices.ElementAt(i);
                                decimal nextPrice = i > 0 ? allOrderPrices.ElementAt(i - 1) : 0;

                                if (prevPrice < potentialNewPrice)
                                {
                                    decimal priceMultiplier = 1;

                                    if (prevPrice * _appSettings.MinRatioToNearestOrder > potentialNewPrice)
                                    {
                                        priceMultiplier = prevPrice;
                                    }
                                    else
                                    {
                                        if (potentialNewPrice > nextPrice / _appSettings.MinRatioToNearestOrder && nextPrice != 0)
                                        {
                                            priceMultiplier = nextPrice;
                                        }
                                        else
                                        {
                                            // There is no need of shifting the order's price.
                                            return potentialNewPrice;
                                        }
                                    }

                                    potentialNewPrice = priceMultiplier / _appSettings.MinRatioToNearestOrder;

                                    for (int j = i; j < allOrderPrices.Count(); j++)
                                    {
                                        decimal prevPrice2 = allOrderPrices.ElementAt(j);
                                        decimal nextPrice2 = j > 0 ? allOrderPrices.ElementAt(j - 1) : 0;

                                        if (prevPrice2 * _appSettings.MinRatioToNearestOrder <= potentialNewPrice && potentialNewPrice <= nextPrice2 / _appSettings.MinRatioToNearestOrder)
                                        {
                                            // There is no need of shifting the order's price.
                                            return potentialNewPrice;
                                        }
                                        else
                                        {
                                            potentialNewPrice = prevPrice2 / _appSettings.MinRatioToNearestOrder;
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    default:
                        throw new ArgumentException($"Unexpected value for OrderSideEnum '{orderSide}' inside the CalculateNewOrderPrice().");
                }

                return potentialNewPrice;
            }
            catch (Exception ex)
            {
                // TODO: Add log

                // There is no need of shifting the order's price.
                return potentialNewPrice;
            }
        }
    }
}