using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using trackingtime.Functions.Functions;
using trackingtime.Test.Helpers;
using Xunit;

namespace trackingtime.Test.Test
{
    public class TimeConsolidatedFunctionTest
    {
        public ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.list);
        //public readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public void TimeConsolidatedFunction_Should_Log_Message()
        {
            // Arrange
            MockCloudTableEmployeeMonitoring mockRecords = new MockCloudTableEmployeeMonitoring(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableTimeConsolidated mockTimeRecords = new MockCloudTableTimeConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            
            // Act
            TimeConsolidatedFunction.Run(null, mockRecords, mockTimeRecords, logger);
            string message = logger.Logs[1];
            // Assart
            Assert.Contains("has been consolidated", message);
        }
    }
}
