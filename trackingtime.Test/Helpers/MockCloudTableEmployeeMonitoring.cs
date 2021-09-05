using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace trackingtime.Test.Helpers
{
    public class MockCloudTableEmployeeMonitoring : CloudTable
    {
        public MockCloudTableEmployeeMonitoring(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableEmployeeMonitoring(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableEmployeeMonitoring(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetEmployeeMonitoringEntity()
            }); ;
        }

        public override async Task<TableQuerySegment<EmployeeMonitoringEntity>> ExecuteQuerySegmentedAsync<EmployeeMonitoringEntity>(TableQuery<EmployeeMonitoringEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<EmployeeMonitoringEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetEmployeeMonitoringEntities() }) as TableQuerySegment<EmployeeMonitoringEntity>);
        }
    }
}
