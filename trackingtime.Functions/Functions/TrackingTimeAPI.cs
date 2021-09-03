using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
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

            if (string.IsNullOrEmpty(trackingEmployee?.Id.ToString()) || string.IsNullOrEmpty(trackingEmployee?.Type.ToString()))
            {
                log.LogInformation("A bad request was returned");
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have an Id and a type."
                });                
            }

            TrackingEmployeeEntity trackingEmployeeEntity = new TrackingEmployeeEntity
            {
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*",
                PartitionKey = "TRACKINGEMPLOYEE",
                Id = trackingEmployee.Id,
                Type = trackingEmployee.Type,
                CreatedDateTime = DateTime.UtcNow,
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
    }
}
