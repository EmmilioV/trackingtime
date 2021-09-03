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
        [FunctionName(nameof(CreateTrackingEmployee))]
        public static async Task<IActionResult> CreateTrackingEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "TrackingTime")] HttpRequest req,
            [Table("TrackingEmployee", Connection = "AzureWebJobsStorage")] CloudTable trackingEmployeeTable,
            ILogger log)
        {
            log.LogInformation("A new tracking employee has been received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrackingEmployee trackingEmployee = JsonConvert.DeserializeObject<TrackingEmployee>(requestBody);

            if (string.IsNullOrEmpty(trackingEmployee?.EmployeeId.ToString()) || string.IsNullOrEmpty(trackingEmployee?.Type.ToString())
                || (!int.Equals(trackingEmployee.Type, 0) && !int.Equals(trackingEmployee.Type, 1)))
            {
                log.LogInformation("A bad request was returned");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a type 0 or 1 and an id."
                });                
            }

            TrackingEmployeeEntity trackingEmployeeEntity = new TrackingEmployeeEntity
            {
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*",
                PartitionKey = "TRACKINGEMPLOYEE",
                EmployeeId = trackingEmployee.EmployeeId,
                Type = trackingEmployee.Type,
                CreatedDateTime = Convert.ToDateTime(trackingEmployee.CreatedDateTime, new CultureInfo("en-US")),
                Consolidated = false
            };

            TableOperation addOperation = TableOperation.Insert(trackingEmployeeEntity);
            await trackingEmployeeTable.ExecuteAsync(addOperation);

            string message = "New tracking employee has been stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = trackingEmployeeEntity
            });
        }

        [FunctionName(nameof(UpdateTrackingEmployee))]
        public static async Task<IActionResult> UpdateTrackingEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "TrackingTime/{recordId}")] HttpRequest req,
            [Table("TrackingEmployee", Connection = "AzureWebJobsStorage")] CloudTable trackingEmployeeTable,
            string recordId,
            ILogger log)
        {
            log.LogInformation($"Update for tracking employee: {recordId}, received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TrackingEmployee trackingEmployee = JsonConvert.DeserializeObject<TrackingEmployee>(requestBody);

            //validate record id 

            TableOperation findOperation = TableOperation.Retrieve<TrackingEmployeeEntity>("TRACKINGEMPLOYEE", recordId);
            TableResult findResult = await trackingEmployeeTable.ExecuteAsync(findOperation);

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
            if(string.IsNullOrEmpty(trackingEmployee?.Type.ToString()) 
               || (!int.Equals(trackingEmployee.Type, 0) && !int.Equals(trackingEmployee.Type, 1)))
            {
                log.LogInformation("A bad request was returned, request must have a type 0 or 1 to update");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "request must have a type 0 or 1, or a date time to update"
                });
            }

            TrackingEmployeeEntity trackingEmployeeEntity = (TrackingEmployeeEntity)findResult.Result;
            trackingEmployeeEntity.Type = trackingEmployee.Type;

            //Update DateTime of record
            if (!string.IsNullOrEmpty(trackingEmployee.CreatedDateTime.ToString()))
            {
                trackingEmployeeEntity.CreatedDateTime = Convert.ToDateTime(trackingEmployee.CreatedDateTime,
                                                                            new CultureInfo("en-US"));
            }


            TableOperation addOperation = TableOperation.Replace(trackingEmployeeEntity);
            await trackingEmployeeTable.ExecuteAsync(addOperation);

            string message = $"The record with id = {recordId}, updated in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = trackingEmployeeEntity
            });
        }

        [FunctionName(nameof(GetAllTrackingEmployee))]
        public static async Task<IActionResult> GetAllTrackingEmployee(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "TrackingTime")] HttpRequest req,
            [Table("TrackingEmployee", Connection = "AzureWebJobsStorage")] CloudTable trackingEmployeeTable,
            ILogger log)
        {
            log.LogInformation("Get all tracking employees received.");

            TableQuery<TrackingEmployeeEntity> query = new TableQuery<TrackingEmployeeEntity>();
            TableQuerySegment<TrackingEmployeeEntity> trackingEmployees = await trackingEmployeeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all tracking employees";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = trackingEmployees
            });
        }
    }
}
