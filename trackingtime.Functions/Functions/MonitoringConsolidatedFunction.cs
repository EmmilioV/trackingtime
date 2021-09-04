using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using trackingtime.Functions.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace trackingtime.Functions.Functions
{
    public static class MonitoringConsolidatedFunction
    {
        [FunctionName("MonitoringConsolidatedFunction")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, 
            [Table("MonitoringConsolidated", Connection = "AzureWebJobsStorage")] CloudTable MonitoringConsolidatedTable,
            [Table("TimeConsolidated", Connection = "AzureWebJobsStorage")] CloudTable TimeConsolidatedTable,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<EmployeeMonitoringEntity> query = new TableQuery<EmployeeMonitoringEntity>().Where(filter);
            TableQuerySegment<EmployeeMonitoringEntity> employeesConsolidated = await MonitoringConsolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            List<EmployeeMonitoringEntity> employeesConsolidatedOrdered = employeesConsolidated
                                                                        .OrderBy(x => x.CreatedDateTime)
                                                                        .OrderBy(x => x.EmployeeId)
                                                                        .ToList();
            int consolidated = 0;
            int i = 1;
            double sumTime = 0;
            foreach (EmployeeMonitoringEntity employeeMonitoringEntity in employeesConsolidatedOrdered)
            {
                if(!employeesConsolidatedOrdered[i].EmployeeId.Equals(employeesConsolidatedOrdered.Last()))
                {
                    if (employeeMonitoringEntity.EmployeeId.Equals(employeesConsolidatedOrdered[i].EmployeeId))
                    {
                    }
                }
            }
        }
    }
}
