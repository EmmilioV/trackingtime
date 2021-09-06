using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using trackingtime.Functions.Entities;

namespace trackingtime.Functions.Functions
{
    public static class TimeConsolidatedFunction
    {
        [FunctionName("MonitoringConsolidatedFunction")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("EmployeeMonitoring", Connection = "AzureWebJobsStorage")] CloudTable employeeMonitoringTable,
            [Table("TimeConsolidated", Connection = "AzureWebJobsStorage")] CloudTable TimeConsolidatedTable,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            TableQuery<EmployeeMonitoringEntity> query = new TableQuery<EmployeeMonitoringEntity>();
            TableQuerySegment<EmployeeMonitoringEntity> employeesConsolidated = await employeeMonitoringTable.ExecuteQuerySegmentedAsync(query, null);

            List<EmployeeMonitoringEntity> employeesConsolidatedOrdered = employeesConsolidated
                                                                        .Where(x => x.Consolidated.Equals(false))
                                                                        .OrderBy(x => x.CreatedDateTime)
                                                                        .OrderBy(x => x.EmployeeId)
                                                                        .ToList();

            int consolidated = 0;
            int i = 1;

            foreach (EmployeeMonitoringEntity employeeMonitoringEntity in employeesConsolidatedOrdered)
            {
                int workedMinutes = 0;
                if (!(employeeMonitoringEntity).Equals(employeesConsolidatedOrdered.Last()))
                {
                    if (employeeMonitoringEntity.EmployeeId.Equals(employeesConsolidatedOrdered[i].EmployeeId) &&
                        employeesConsolidatedOrdered[i].Type.Equals(1))
                    {
                        TimeSpan workedTime = employeesConsolidatedOrdered[i].CreatedDateTime - employeeMonitoringEntity.CreatedDateTime;
                        workedMinutes = (int)workedTime.TotalMinutes;

                        employeesConsolidatedOrdered[i].Consolidated = true;
                        employeeMonitoringEntity.Consolidated = true;

                        //Update consolidated of actual record
                        TableOperation addOperation = TableOperation.Replace(employeeMonitoringEntity);
                        await employeeMonitoringTable.ExecuteAsync(addOperation);

                        //Update consolidated of next record
                        TableOperation addOperation2 = TableOperation.Replace(employeesConsolidatedOrdered[i]);
                        await employeeMonitoringTable.ExecuteAsync(addOperation2);

                        string filter = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, employeeMonitoringEntity.EmployeeId);
                        TableQuery<TimeConsolidatedEntity> q = new TableQuery<TimeConsolidatedEntity>().Where(filter);
                        TableQuerySegment<TimeConsolidatedEntity> timeConsolidated = await TimeConsolidatedTable.ExecuteQuerySegmentedAsync(q, null);

                        if (timeConsolidated.Results.Count > 0)
                        {
                            timeConsolidated.Results.FirstOrDefault().WorkedMinutes += workedMinutes;

                            TableOperation addOperation3 = TableOperation.Replace(timeConsolidated.Results.FirstOrDefault());
                            await TimeConsolidatedTable.ExecuteAsync(addOperation3);
                        }
                        else
                        {
                            TimeConsolidatedEntity timeConsolidatedEntity = new TimeConsolidatedEntity
                            {
                                EmployeeId = employeeMonitoringEntity.EmployeeId,
                                Date = DateTime.UtcNow,
                                WorkedMinutes = workedMinutes,
                                ETag = "*",
                                RowKey = Guid.NewGuid().ToString(),
                                PartitionKey = "TIMECONSOLIDATED"
                            };

                            TableOperation addOperation4 = TableOperation.Insert(timeConsolidatedEntity);
                            await TimeConsolidatedTable.ExecuteAsync(addOperation4);
                        }
                        consolidated++;
                    }
                }
                i++;
            }

            log.LogInformation($"{consolidated} records of employee monitoring has been consolidated. ");

        }


    }
}
