using System;
using System.Collections.Generic;
using AT.Business.Interfaces;
using AT.Business.Models.Exchange;
using Xunit;

namespace AT.Tests
{
    public class TraderHelperServiceTests
    {
        private readonly ITraderHelperService _traderHelperService;

        public TraderHelperServiceTests(ITraderHelperService traderHelperService)
        {
            _traderHelperService = traderHelperService;
        }

        [Theory]
        [InlineData(0.071)]

        [InlineData(0.0787)]
        [InlineData(0.0789)]
        [InlineData(0.0793)]

        [InlineData(0.0995)]
        [InlineData(0.1)]
        [InlineData(0.10037)]

        [InlineData(0.137)]
        [InlineData(0.13715)]
        [InlineData(0.13777)]
        [InlineData(0.1381)]
        [InlineData(0.1383)]

        [InlineData(0.17)]
        [InlineData(0.1705)]
        [InlineData(0.171)]
        [InlineData(0.1717)]
        [InlineData(0.172)]

        [InlineData(0.2379)]
        [InlineData(0.239)]
        [InlineData(0.2405)]

        [InlineData(0.289)]
        [InlineData(0.29)]
        [InlineData(0.291)]

        [InlineData(0.379)]
        public void CalculateNewOrderPrice_Sell_Test(decimal potentialNewPrice)
        {
            // Arrange

            // A deliberately scrambled sheet of values.
            var sellOrderPricesList = new List<decimal>
            {
                0.174M,
                0.18M,
                0.191M,
                0.1M,
                0.1M,
                0.0789M,
                0.0789M,
                0.0789M,
                0.123M,
                0.153M,
                0.17M,
                0.172M,
                0.172M,
                0.176M,
                0.178M,
                0.29M,
                0.29M,
                0.23M,
                0.137M,
                0.1383M,
                0.1383M,
                0.187M,
                0.187M,
                0.1495M,
                0.239M,
            };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, sellOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 0.071M:
                case 0.379M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 0.0787M:
                case 0.0789M:
                case 0.0793M:
                    Assert.Equal(0.07944M, Math.Round(newPrice, 5));
                    break;

                case 0.0995M:
                case 0.1M:
                case 0.10037M:
                    Assert.Equal(0.10069M, Math.Round(newPrice, 5));
                    break;

                case 0.137M:
                case 0.13715M:
                case 0.13777M:
                case 0.1381M:
                case 0.1383M:
                    Assert.Equal(0.13925M, Math.Round(newPrice, 5));
                    break;

                case 0.17M:
                case 0.1705M:
                case 0.171M:
                case 0.1717M:
                case 0.172M:
                    Assert.Equal(0.18124M, Math.Round(newPrice, 5));
                    break;

                case 0.2379M:
                case 0.239M:
                case 0.2405M:
                    Assert.Equal(0.24065M, Math.Round(newPrice, 5));
                    break;

                case 0.289M:
                case 0.29M:
                case 0.291M:
                    Assert.Equal(0.292M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(0.11)]
        [InlineData(0.1355)]
        [InlineData(0.1357)]
        [InlineData(0.1363)]
        [InlineData(0.15)]
        public void CalculateNewOrderPrice_Sell_One_OrderPrice_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var sellOrderPricesList = new List<decimal> { 0.1357M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, sellOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 0.11M:
                case 0.15M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 0.1355M:
                case 0.1357M:
                case 0.1363M:
                    Assert.Equal(0.13664M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(0.11)]
        [InlineData(0.1355)]
        [InlineData(0.1357)]
        [InlineData(0.1363)]
        [InlineData(0.15)]
        public void CalculateNewOrderPrice_Sell_Two_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var sellOrderPricesList = new List<decimal> { 0.1357M, 0.147M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, sellOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 0.11M:
                case 0.15M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 0.1355M:
                case 0.1357M:
                case 0.1363M:
                    Assert.Equal(0.13664M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(0.11)]
        [InlineData(0.1355)]
        [InlineData(0.1357)]
        [InlineData(0.1363)]
        [InlineData(0.15)]
        public void CalculateNewOrderPrice_Sell_Three_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var sellOrderPricesList = new List<decimal> { 0.0975M, 0.1357M, 0.17M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, sellOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 0.11M:
                case 0.15M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 0.1355M:
                case 0.1357M:
                case 0.1363M:
                    Assert.Equal(0.13664M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(0.11)]
        [InlineData(0.1355)]
        [InlineData(0.1357)]
        [InlineData(0.1363)]
        [InlineData(0.15)]
        public void CalculateNewOrderPrice_Sell_Two_Equal_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var sellOrderPricesList = new List<decimal> { 0.1357M, 0.1357M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, sellOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 0.11M:
                case 0.15M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 0.1355M:
                case 0.1357M:
                case 0.1363M:
                    Assert.Equal(0.13664M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Fact]
        public void CalculateNewOrderPrice_Sell_Empty_OrderPricesList_Test()
        {
            // Arrange
            decimal potentialNewPrice = 12.345M;

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, new List<decimal>());

            // Assert
            Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
        }

        [Fact]
        public void CalculateNewOrderPrice_Sell_Null_OrderPricesList_Test()
        {
            // Arrange
            decimal potentialNewPrice = 12.345M;

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Sell, potentialNewPrice, null);

            // Assert
            Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
        }

        [Theory]
        [InlineData(139.13)]

        [InlineData(123.89)]
        [InlineData(123.1)]
        [InlineData(122.33)]

        [InlineData(111.79)]
        [InlineData(111.1)]
        [InlineData(110.67)]

        [InlineData(63.3)]
        [InlineData(63.13)]
        [InlineData(62.89)]
        [InlineData(62.61)]
        [InlineData(62.49)]

        [InlineData(31.73)]
        [InlineData(31.57)]
        [InlineData(31.53)]
        [InlineData(31.37)]
        [InlineData(31.33)]

        [InlineData(23.81)]
        [InlineData(23.7)]
        [InlineData(23.579)]

        [InlineData(21.61)]
        [InlineData(21.5)]
        [InlineData(21.39)]

        [InlineData(11.1)]
        public void CalculateNewOrderPrice_Buy_Test(decimal potentialNewPrice)
        {
            // Arrange

            // A deliberately scrambled sheet of values.
            var buyOrderPricesList = new List<decimal>()
            {
                31.33M,
                123.1M,
                123.1M,
                123.1M,
                61.3M,
                103.7M,
                99.9M,
                67.79M,
                61.9M,
                59.9M,
                39.9M,
                39.9M,
                35.1M,
                31.73M,
                31.73M,
                63.3M,
                29.69M,
                29.69M,
                62.49M,
                62.49M,
                23.7M,
                21.5M,
                21.5M,
                111.1M,
                60.6M,
            };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, buyOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 139.13M:
                case 11.1M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 123.89M:
                case 123.1M:
                case 122.33M:
                    Assert.Equal(122.25643M, Math.Round(newPrice, 5));
                    break;

                case 111.79M:
                case 111.1M:
                case 110.67M:
                    Assert.Equal(110.33866M, Math.Round(newPrice, 5));
                    break;

                case 63.3M:
                case 63.13M:
                case 62.89M:
                case 62.61M:
                case 62.49M:
                    Assert.Equal(59.48952M, Math.Round(newPrice, 5));
                    break;

                case 31.73M:
                case 31.57M:
                case 31.53M:
                case 31.37M:
                case 31.33M:
                    Assert.Equal(31.1153M, Math.Round(newPrice, 5));
                    break;

                case 23.81M:
                case 23.7M:
                case 23.579M:
                    Assert.Equal(23.53759M, Math.Round(newPrice, 5));
                    break;

                case 21.61M:
                case 21.5M:
                case 21.39M:
                    Assert.Equal(21.35267M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(13.13)]
        [InlineData(9.813)]
        [InlineData(9.753)]
        [InlineData(9.71)]
        [InlineData(5.79)]
        public void CalculateNewOrderPrice_Buy_One_OrderPrice_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var buyOrderPricesList = new List<decimal> { 9.753M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, buyOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 13.13M:
                case 5.79M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 9.813M:
                case 9.753M:
                case 9.71M:
                    Assert.Equal(9.68617M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(13.13)]
        [InlineData(9.813)]
        [InlineData(9.753)]
        [InlineData(9.71)]
        [InlineData(5.79)]
        public void CalculateNewOrderPrice_Buy_Two_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var buyOrderPricesList = new List<decimal> { 9.753M, 9.81M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, buyOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 13.13M:
                case 5.79M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 9.813M:
                case 9.753M:
                case 9.71M:
                    Assert.Equal(9.68617M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(13.13)]
        [InlineData(9.813)]
        [InlineData(9.753)]
        [InlineData(9.71)]
        [InlineData(5.79)]
        public void CalculateNewOrderPrice_Buy_Three_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var buyOrderPricesList = new List<decimal> { 9.753M, 7.77M, 3.57M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, buyOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 13.13M:
                case 5.79M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 9.813M:
                case 9.753M:
                case 9.71M:
                    Assert.Equal(9.68617M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Theory]
        [InlineData(13.13)]
        [InlineData(9.813)]
        [InlineData(9.753)]
        [InlineData(9.71)]
        [InlineData(5.79)]
        public void CalculateNewOrderPrice_Buy_Two_Equal_OrderPrices_In_List_Test(decimal potentialNewPrice)
        {
            // Arrange
            var buyOrderPricesList = new List<decimal> { 9.753M, 9.753M };

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, buyOrderPricesList);

            // Assert
            switch (potentialNewPrice)
            {
                case 13.13M:
                case 5.79M:
                    Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
                    break;

                case 9.813M:
                case 9.753M:
                case 9.71M:
                    Assert.Equal(9.68617M, Math.Round(newPrice, 5));
                    break;

                default:
                    Assert.True(false, "The value of InlineData is not supported in the test cases.");
                    break;
            }
        }

        [Fact]
        public void CalculateNewOrderPrice_Buy_Empty_OrderPricesList_Test()
        {
            // Arrange
            decimal potentialNewPrice = 12.345M;

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, new List<decimal>());

            // Assert
            Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
        }

        [Fact]
        public void CalculateNewOrderPriceBuy_Null_OrderPricesList_Test()
        {
            // Arrange
            decimal potentialNewPrice = 12.345M;

            // Act
            var newPrice = _traderHelperService.CalculateNewOrderPrice(OrderSideEnum.Buy, potentialNewPrice, null);

            // Assert
            Assert.Equal(potentialNewPrice, Math.Round(newPrice, 5));
        }
    }
}
