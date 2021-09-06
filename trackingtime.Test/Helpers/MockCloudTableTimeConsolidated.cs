using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using trackingtime.Functions.Entities;

namespace trackingtime.Test.Helpers
{
    public class MockCloudTableTimeConsolidated : CloudTable
    {
        public MockCloudTableTimeConsolidated(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableTimeConsolidated(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableTimeConsolidated(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
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

        public override async Task<TableQuerySegment<TimeConsolidatedEntity>> ExecuteQuerySegmentedAsync<TimeConsolidatedEntity>(TableQuery<TimeConsolidatedEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<TimeConsolidatedEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetTimeConsolidatedEntities() }) as TableQuerySegment<TimeConsolidatedEntity>);
        }
    }
}
