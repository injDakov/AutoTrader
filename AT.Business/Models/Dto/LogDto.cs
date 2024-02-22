using System;
using AT.Business.Enums;
using AT.Domain.Enums;

namespace AT.Business.Models.Dto
{
    public class LogDto
    {
        public LogDto(LogType type, string action, DetailedMessage detailedMessage, LogSourceType sourceType = LogSourceType.Default, string @event = null)
        {
            Type = type;
            Action = action;
            DetailedMessage = detailedMessage;
            SourceType = sourceType;
            Event = @event;
        }

        public DateTime CreateDate { get; } = DateTime.UtcNow;

        public LogType Type { get; set; }

        public string Event { get; set; }

        public string Action { get; set; }

        public DetailedMessage DetailedMessage { get; set; }

        public LogSourceType SourceType { get; set; }
    }
}