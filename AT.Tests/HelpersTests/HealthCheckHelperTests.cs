using System;
using System.Collections.Generic;
using AT.Business.Helpers;
using Xunit;

namespace AT.Tests.HelpersTests
{
    public class HealthCheckHelperTests
    {
        [Theory]
        [InlineData("2023-10-04 00:00:11", 0, new int[] { 5, 12, 21 })]
        [InlineData("2023-10-04 17:14:11", 17, new int[] { 5, 12, 21 })]
        [InlineData("2023-10-04 17:14:11", 0, new int[] { })]
        [InlineData("2023-10-04 17:14:11", 3, null)]
        [InlineData("2023-10-04 17:14:11", 0, new int[] { 5, 12, 21 })]
        public void IsItTime_PassingTest_False(string dateTimeStr, int healthCheckIntervalInHours, IEnumerable<int> healthCheckHours)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeStr);

            // Act
            var res = HealthCheckHelper.IsItTime(dateTime, healthCheckIntervalInHours, healthCheckHours);

            // Assert
            Assert.False(res);
        }

        [Theory]
        [InlineData("2023-10-04 00:00:11", 0, new int[] { 0, 5, 12, 21 })]
        [InlineData("2023-10-04 12:00:11", 17, new int[] { 5, 12, 21 })]
        [InlineData("2023-10-04 17:00:11", 17, new int[] { 5, 12, 21 })]
        [InlineData("2023-10-04 17:00:11", 17, new int[] { })]
        [InlineData("2023-10-04 17:00:11", 17, null)]
        [InlineData("2023-10-04 21:00:11", 17, new int[] { 5, 12, 21 })]
        public void IsItTime_PassingTest_True(string dateTimeStr, int healthCheckIntervalInHours, IEnumerable<int> healthCheckHours)
        {
            // Arrange
            var dateTime = DateTime.Parse(dateTimeStr);

            // Act
            var res = HealthCheckHelper.IsItTime(dateTime, healthCheckIntervalInHours, healthCheckHours);

            // Assert
            Assert.True(res);
        }
    }
}