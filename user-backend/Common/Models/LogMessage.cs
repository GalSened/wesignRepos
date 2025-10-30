using Common.Enums.Logs;
using System;

namespace Common.Models
{
    public class LogMessage
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Exception { get; set; }
    }
}
