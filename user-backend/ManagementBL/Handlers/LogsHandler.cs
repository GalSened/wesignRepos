using Common.Enums.Logs;
using Common.Interfaces.DB;
using Common.Interfaces.ManagementApp;
using Common.Models;
using System.Collections.Generic;

namespace ManagementBL.Handlers
{
    public class LogsHandler : ILogs
    {
        private readonly ILogConnector _logConnector;

        public LogsHandler(ILogConnector logConnector)
        {
            _logConnector = logConnector;
        }

        public IEnumerable<LogMessage> Read(LogApplicationSource source, string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount)
        {
            return _logConnector.Read(source, key, from, to, logLevel, offset, limit, out totalCount);
        }
    }
}
