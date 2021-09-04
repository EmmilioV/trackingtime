using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace trackingtime.Functions.Entities
{
    internal class TimeConsolidatedEntity : TableEntity
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public int WorkedMinutes { get; set; }
    }
}
