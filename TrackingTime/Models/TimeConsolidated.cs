using System;

namespace trackingtime.Common.Models
{
    public class TimeConsolidated
    {
        public int EmployeeId { get; set; }

        public DateTime Date { get; set; }

        public int WorkedMinutes { get; set; }
    }
}
