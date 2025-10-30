using Common.Enums.Logs;
using Common.Models;
using System;

namespace WeSignManagement.Models.Logs
{
    public class LogMessageDTO
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public LogMessageDTO(LogMessage log)
        {
            LogLevel = log.LogLevel;
            Message = log.Message;
            TimeStamp = log.TimeStamp;
        }
    }
}
