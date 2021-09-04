using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using trackingtime.Common.Models;
using trackingtime.Common.Responses;
using trackingtime.Functions.Entities;

namespace trackingtime.Functions.Functions
{
    public static class TrackingTimeAPI
    {
        [FunctionName(nameof(CreateEmployeeRecord))]
        public static async Task<IActionResult> CreateEmployeeRecord(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TrackingTime")] HttpRequest req,
            [Table("EmployeeMonitoring", Connection = "AzureWebJobsStorage")] CloudTable employeeMonitoringTable,
            ILogger log)
        {
            log.LogInformation("A new employee monitoring record has been received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeMonitoring employeeMonitoring = JsonConvert.DeserializeObject<EmployeeMonitoring>(requestBody);

            if (string.IsNullOrEmpty(employeeMonitoring?.EmployeeId.ToString()) || string.IsNullOrEmpty(employeeMonitoring?.Type.ToString())
                || (!int.Equals(employeeMonitoring.Type, 0) && !int.Equals(employeeMonitoring.Type, 1)))
            {
                log.LogInformation("A bad request was returned");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a type 0 or 1 and an id."
                });                
            }

            EmployeeMonitoringEntity employeeMonitoringEntity = new EmployeeMonitoringEntity
            {
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*",
                PartitionKey = "EMPLOYEERECORD",
                EmployeeId = employeeMonitoring.EmployeeId,
                Type = employeeMonitoring.Type,
                CreatedDateTime = Convert.ToDateTime(employeeMonitoring.CreatedDateTime, new CultureInfo("en-US")),
                Consolidated = false
            };

            TableOperation addOperation = TableOperation.Insert(employeeMonitoringEntity);
            await employeeMonitoringTable.ExecuteAsync(addOperation);

            string message = "New employee time monitoring record has been stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoringEntity
            });
        }

        [FunctionName(nameof(UpdateTrackingEmployee))]
        public static async Task<IActionResult> UpdateTrackingEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "TrackingTime/{recordId}")] HttpRequest req,
            [Table("EmployeeMonitoring", Connection = "AzureWebJobsStorage")] CloudTable employeeMonitoringTable,
            string recordId,
            ILogger log)
        {
            log.LogInformation($"Update for employee monitoring record: {recordId}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            EmployeeMonitoring employeeMonitoring = JsonConvert.DeserializeObject<EmployeeMonitoring>(requestBody);

            //validate record id 

            TableOperation findOperation = TableOperation.Retrieve<EmployeeMonitoringEntity>("EMPLOYEERECORD", recordId);
            TableResult findResult = await employeeMonitoringTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                log.LogInformation("A bad request was returned, the record with id requested NOT found");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "the record with id requested NOT found"
                });
            }

            //Update type of record
            if(string.IsNullOrEmpty(employeeMonitoring?.Type.ToString()) 
               || (!int.Equals(employeeMonitoring.Type, 0) && !int.Equals(employeeMonitoring.Type, 1)))
            {
                log.LogInformation("A bad request was returned, request must have a type 0 or 1 to update");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "request must have a type 0 or 1, or a date time to update"
                });
            }

            EmployeeMonitoringEntity employeeMonitoringEntity = (EmployeeMonitoringEntity)findResult.Result;
            employeeMonitoringEntity.Type = employeeMonitoring.Type;

            //Update DateTime of record
            if (!string.IsNullOrEmpty(employeeMonitoring.CreatedDateTime.ToString()))
            {
                employeeMonitoringEntity.CreatedDateTime = Convert.ToDateTime(employeeMonitoring.CreatedDateTime,
                                                                            new CultureInfo("en-US"));
            }


            TableOperation addOperation = TableOperation.Replace(employeeMonitoringEntity);
            await employeeMonitoringTable.ExecuteAsync(addOperation);

            string message = $"The record with id = {recordId}, updated in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoringEntity
            });
        }

        [FunctionName(nameof(GetAllTrackingEmployee))]
        public static async Task<IActionResult> GetAllTrackingEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TrackingTime")] HttpRequest req,
            [Table("EmployeeMonitoring", Connection = "AzureWebJobsStorage")] CloudTable employeeMonitoringTable,
            ILogger log)
        {
            log.LogInformation("Get all time monitoring of employees received.");

            TableQuery<EmployeeMonitoringEntity> query = new TableQuery<EmployeeMonitoringEntity>();
            TableQuerySegment<EmployeeMonitoringEntity> employeeMonitoring = await employeeMonitoringTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all tracking employees";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoring
            });
        }

        [FunctionName(nameof(GetEmployeeTrackingById))]
        public static IActionResult GetEmployeeTrackingById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TrackingTime/{recordId}")] HttpRequest req,
            [Table("EmployeeMonitoring", "EMPLOYEERECORD", "{recordId}", Connection = "AzureWebJobsStorage")] EmployeeMonitoringEntity employeeMonitoringTable,
            string recordId,
            ILogger log)
        {
            log.LogInformation($"Get record of employee monitoring by Id:{recordId}, received.");

            if (employeeMonitoringTable == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "record of employee tracking NOT found."
                });
            }

            string message = $"record of employee monitoring with id: {employeeMonitoringTable.RowKey}, retrieved.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoringTable
            });
        }

        [FunctionName(nameof(DeleteEmployeeTracking))]
        public static async Task<IActionResult> DeleteEmployeeTracking(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Delete", Route = "TrackingTime/{recordId}")] HttpRequest req,
            [Table("EmployeeMonitoring", "EMPLOYEERECORD", "{recordId}", Connection = "AzureWebJobsStorage")] EmployeeMonitoringEntity employeeMonitoringEntity,
            [Table("EmployeeMonitoring", Connection = "AzureWebJobsStorage")] CloudTable employeeMonitoringTable,
            string recordId,
            ILogger log)
        {
            log.LogInformation($"record of employee monitoring by Id:{recordId}, received.");

            if (employeeMonitoringEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "record of employee monitoring NOT found."
                });
            }

            await employeeMonitoringTable.ExecuteAsync(TableOperation.Delete(employeeMonitoringEntity));

            string message = $"record of employee monitoring by Id: {employeeMonitoringEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = employeeMonitoringEntity
            });
        }

    }
}
