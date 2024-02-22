using System;
using System.Collections.Generic;
using AT.Business.Models.AppSettings;
using Xunit;

namespace AT.Tests.ModelTests
{
    public class AppSettingsTests
    {
        [Theory]
        [InlineData(1, 0, 2, new int[] { 1, 7, 11, 15 }, 3, 1.01, 1.03)]
        [InlineData(123, 5, 9, new int[] { }, 321, 7, 11)]
        [InlineData(3210, 11, 23, new int[] { 9 }, 1234, 1.111, 1.113)]
        public void Set_All_AppSettings_PassingTest(int timeBetweenIterationInSeconds, int cacheExpirationMultiplier, int healthCheckIntervalInHours, IEnumerable<int> healthCheckHours, int pricesQueueSize, decimal profitRatio, decimal minRatioToNearestOrder)
        {
            // Arrange
            var appSettings = new AppSettings
            {
                TimeBetweenIterationInSeconds = timeBetweenIterationInSeconds,
                CacheExpirationMultiplier = cacheExpirationMultiplier,
                HealthCheckIntervalInHours = healthCheckIntervalInHours,
                HealthCheckHours = healthCheckHours,
                PricesQueueSize = pricesQueueSize,
                ProfitRatio = profitRatio,
                MinRatioToNearestOrder = minRatioToNearestOrder,
            };

            // Act and Assert
            Assert.Equal(appSettings.TimeBetweenIterationInSeconds, timeBetweenIterationInSeconds);
            Assert.Equal(appSettings.CacheExpirationMultiplier, cacheExpirationMultiplier);
            Assert.Equal(appSettings.HealthCheckIntervalInHours, healthCheckIntervalInHours);
            Assert.Equal(appSettings.HealthCheckHours, healthCheckHours);
            Assert.Equal(appSettings.PricesQueueSize, pricesQueueSize);
            Assert.Equal(appSettings.ProfitRatio, profitRatio);
            Assert.Equal(appSettings.MinRatioToNearestOrder, minRatioToNearestOrder);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        [InlineData(-321)]
        public void Set_TimeBetweenIterationInSeconds_FailingTest(int timeBetweenIterationInSeconds)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.TimeBetweenIterationInSeconds = timeBetweenIterationInSeconds);
        }

        [Theory]
        [InlineData(-3)]
        [InlineData(-10)]
        public void Set_CacheExpirationMultiplier_FailingTest(int cacheExpirationMultiplier)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.CacheExpirationMultiplier = cacheExpirationMultiplier);
        }

        [Theory]
        [InlineData(-7)]
        [InlineData(25)]
        public void Set_HealthCheckIntervalInHours_FailingTest(int healthCheckIntervalInHours)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.HealthCheckIntervalInHours = healthCheckIntervalInHours);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new int[] { -7 })]
        [InlineData(new int[] { 24 })]
        [InlineData(new int[] { 11, 15, 15, 17 })]
        public void Set_HealthCheckHours_FailingTest(IEnumerable<int> healthCheckHours)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.HealthCheckHours = healthCheckHours);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        [InlineData(-321)]
        public void Set_PricesQueueSize_FailingTest(int pricesQueueSize)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.PricesQueueSize = pricesQueueSize);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.9)]
        [InlineData(-1.1)]
        public void Set_ProfitRatio_FailingTest(decimal profitRatio)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.ProfitRatio = profitRatio);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(0.9)]
        [InlineData(-1.1)]
        public void Set_MinRatioToNearestOrder_FailingTest(decimal minRatioToNearestOrder)
        {
            // Arrange
            var appSettings = new AppSettings();

            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => appSettings.MinRatioToNearestOrder = minRatioToNearestOrder);
        }
    }
}