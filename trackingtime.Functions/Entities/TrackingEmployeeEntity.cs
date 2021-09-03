using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace trackingtime.Functions.Entities
{
    public class TrackingEmployeeEntity : TableEntity
    {
        public int EmployeeId { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
