using Common.Enums.Logs;
using Common.Models;
using DAL.DAOs.Logs;
using System;

namespace DAL.Extensions
{
    public static class SignerLogExtentions
    {
        public static LogMessage ToLog(this SignerLogDAO input)
        {
            if (input == null)
            {
                return null;
            }
            Enum.TryParse(input.Level, out LogLevel logLevel);
            return new LogMessage
            {
                Message = input.Message,
                MessageTemplate = input.MessageTemplate,
                Exception = input.Exception,
                LogLevel = logLevel,
                TimeStamp = input.TimeStamp
            };

        }
    }
}
