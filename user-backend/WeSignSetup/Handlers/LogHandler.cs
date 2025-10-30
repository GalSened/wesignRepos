using System;
using System.Reflection;
using System.Windows.Controls;

namespace WeSignSetup.Handlers
{

    public class LogHandler
    {
        private readonly log4net.ILog _logger;
        private RichTextBox _uiLogs;

        public LogHandler(RichTextBox uiLogs)
        {
            _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            _uiLogs = uiLogs;
        }

        public void Debug(string message, bool shouldLogToUI = true)
        {
            _logger.Debug(message);
            if (shouldLogToUI)
            {
                _uiLogs.AppendText($"{Environment.NewLine}Debug | {message}");             
            }
        }
        public void Info(string message, bool shouldLogToUI = true)
        {
            _logger.Info(message);
            if (shouldLogToUI)
            {
                _uiLogs.AppendText($"{Environment.NewLine}Info | {message}");               
            }
        }

        public void Error(string message, Exception ex, bool shouldLogToUI = true)
        {
            _logger.Error(message, ex);
            if (shouldLogToUI)
            {
                _uiLogs.AppendText($"{Environment.NewLine}Error | {message}, {ex}");               
            }
        }

        public void Warn(string message, bool shouldLogToUI = true)
        {
            _logger.Debug(message);
            if (shouldLogToUI)
            {
                _uiLogs.AppendText($"{Environment.NewLine}Warn | {message}");
            }            
        }
    }
}
