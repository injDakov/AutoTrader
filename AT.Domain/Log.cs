using System;
using AT.Domain.Enums;

namespace AT.Domain
{
    public class Log
    {
        public long Id { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public LogType Type { get; set; }

        public string Action { get; set; }

        public string Message { get; set; }

        public string DetailedMessage { get; set; }
    }
}