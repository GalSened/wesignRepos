using System.Collections.Generic;
using Common.Enums.Logs;
using Common.Models;

namespace Common.Interfaces.ManagementApp
{
    public interface ILogs
    {
        IEnumerable<LogMessage> Read(LogApplicationSource source, string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount);
    }
}
