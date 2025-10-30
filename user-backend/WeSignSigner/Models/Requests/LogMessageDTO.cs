using Common.Enums.Logs;
using System;

namespace WeSignSigner.Models.Requests
{
    public class LogMessageDTO
    {
        public string Token { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public string ApplicationName { get; set; }
        public string Exception { get; set; }
        public string ClientIP { get; set; }
    }
}
