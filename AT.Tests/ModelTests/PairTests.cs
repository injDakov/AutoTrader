using System;
using AT.Business.Models.AppSettings;
using Xunit;

namespace AT.Tests.ModelTests
{
    public class PairTests
    {
        [Theory]
        [InlineData("BTC", 1, 3)]
        [InlineData("BTCUSD", 100, 9)]
        [InlineData("tBTC", 0.111, 33)]
        public void Set_All_Pair_PassingTest(string name, decimal orderAmount, int maxOrderLevelCount)
        {
            // Arrange
            var pair = new Pair
            {
                Name = name,
                OrderAmount = orderAmount,
                MaxOrderLevelCount = maxOrderLevelCount,
            };

            // Act and Assert
            Assert.Equal(pair.Name, name);
            Assert.Equal(pair.OrderAmount, orderAmount);
            Assert.Equal(pair.MaxOrderLevelCount, maxOrderLevelCount);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("")]
        public void Set_Name_FailingTest(string name)
        {
            // Arrange
            var pair = new Pair();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => pair.Name = name);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-0.01)]
        [InlineData(-1.1)]
        public void Set_OrderAmount_FailingTest(decimal orderAmount)
        {
            // Arrange
            var pair = new Pair();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => pair.OrderAmount = orderAmount);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-7)]
        [InlineData(-321)]
        public void Set_MaxOrderLevel_FailingTest(int maxOrderLevelCount)
        {
            // Arrange
            var pair = new Pair();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => pair.MaxOrderLevelCount = maxOrderLevelCount);
        }
    }
}