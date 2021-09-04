using System;
using System.Collections.Generic;
using System.Text;

namespace trackingtime.Common.Models
{
    public class EmployeeMonitoring
    {
        public int EmployeeId { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public int Type { get; set; }

        public bool Consolidated { get; set; }
    }
}
