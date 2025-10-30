using Common.Enums.Logs;
using Common.Models;
using System;
using System.Collections.Generic;

namespace Common.Interfaces.DB
{
    public interface ILogConnector
    {
        IEnumerable<LogMessage> Read(LogApplicationSource source, string key, string from, string to, LogLevel logLevel, int offset, int limit, out int totalCount);


        /// <summary>
        /// Delete all old logs start from "from" backward in time 
        /// </summary>
        /// <param name="from"></param>
        void Delete(DateTime from, LogApplicationSource source);
    }

}
