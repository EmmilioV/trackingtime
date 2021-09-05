using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using trackingtime.Common.Models;
using trackingtime.Functions.Entities;

namespace trackingtime.Test.Helpers
{
    public class TestFactory
    {
        public static EmployeeMonitoringEntity GetEmployeeMonitoringEntity()
        {
            return new EmployeeMonitoringEntity
            {
                ETag = "*",
                PartitionKey = "EMPLOYEERECORD",
                RowKey = Guid.NewGuid().ToString(),
                EmployeeId = new Random().Next(1, 6),
                Type = new Random().Next(0, 2),
                CreatedDateTime = DateTime.UtcNow,
                Consolidated = false,
            };
        }

        //public static EmployeeMonitoringEntity GetEmployeeMonitoringEntity()
        //{
        //    return new EmployeeMonitoringEntity
        //    {
        //        ETag = "*",
        //        PartitionKey = "EMPLOYEERECORD",
        //        RowKey = Guid.NewGuid().ToString(),
        //        EmployeeId = new Random().Next(1, 6),
        //        Type = new Random().Next(0, 2),
        //        CreatedDateTime = DateTime.UtcNow,
        //        Consolidated = false,
        //    };
        //}

        public static List<EmployeeMonitoringEntity> GetEmployeeMonitoringEntities()
        {
            return new List<EmployeeMonitoringEntity>();
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid recordId, EmployeeMonitoring employeeRequest)
        {
            string request = JsonConvert.SerializeObject(employeeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{recordId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid recordId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{recordId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(EmployeeMonitoring employeeMonitoringRequest)
        {
            string request = JsonConvert.SerializeObject(employeeMonitoringRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static EmployeeMonitoring GetEmployeeMonitoringRequest()
        {
            return new EmployeeMonitoring
            {
                EmployeeId = new Random().Next(1, 6),
                Type = new Random().Next(0, 2),
                CreatedDateTime = DateTime.UtcNow,
                Consolidated = false,
            };
        }

        public static Stream GenerateStreamFromString(string stringToConver)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConver);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.list)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null logger");
            }

            return logger;
        }
    }
}
