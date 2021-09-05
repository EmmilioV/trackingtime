
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using trackingtime.Common.Models;
using trackingtime.Functions.Entities;
using trackingtime.Functions.Functions;
using trackingtime.Test.Helpers;
using Xunit;

namespace trackingtime.Test.Test
{
    public class TrackingTimeApiTest
    {
        public readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreatedMonitoringEmployee_Should_Return200()
        {
            // Arrange
            MockCloudTableEmployeeMonitoring mockRecords = new MockCloudTableEmployeeMonitoring(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeMonitoring employeeMonitoringRequest = TestFactory.GetEmployeeMonitoringRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(employeeMonitoringRequest);

            // Act
            IActionResult response = await TrackingTimeAPI.CreateEmployeeRecord(request, mockRecords, logger);

            // Assart
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateMonitoringEmployee_Should_Return200()
        {
            // Arrange
            MockCloudTableEmployeeMonitoring mockRecords = new MockCloudTableEmployeeMonitoring(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeMonitoring employeeMonitoringRequest = TestFactory.GetEmployeeMonitoringRequest();
            Guid recordId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(recordId, employeeMonitoringRequest);

            // Act
            IActionResult response = await TrackingTimeAPI.UpdateTrackingEmployee(request, mockRecords, recordId.ToString(), logger);

            // Assart
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void GetMonitoringEmployee_Should_Return200()
        {
            // Arrange
            EmployeeMonitoringEntity employeeMonitoringEntity = TestFactory.GetEmployeeMonitoringEntity();
            Guid recordId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(recordId);

            // Act
            IActionResult response = TrackingTimeAPI.GetEmployeeTrackingById(request, employeeMonitoringEntity, recordId.ToString(), logger);

            // Assart
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllMonitoringEmployee_Should_Return200()
        {
            // Arrange
            MockCloudTableEmployeeMonitoring mockRecords = new MockCloudTableEmployeeMonitoring(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            // Act
            IActionResult response = await TrackingTimeAPI.GetAllTrackingEmployee(request, mockRecords, logger);

            // Assart
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeletedMonitoringEmployee_Should_Return200()
        {
            // Arrange
            MockCloudTableEmployeeMonitoring mockRecords = new MockCloudTableEmployeeMonitoring(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeMonitoringEntity employeeMonitoringEntity = TestFactory.GetEmployeeMonitoringEntity();
            Guid recordId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(recordId);

            // Act
            IActionResult response = await TrackingTimeAPI.DeleteEmployeeTracking(request, employeeMonitoringEntity, mockRecords, recordId.ToString(), logger);

            // Assart
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
