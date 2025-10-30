using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartCardDesktopClient
{
    public class Logger
    {
        protected const string MEDIA_TYPE = "application/json";
        protected const string APPLICATION_NAME = "Smart Card Desktop Client";
        private readonly bool _shouldWriteToLogServer;
        private readonly string _url;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string _localIP;

        public Logger()
        {
            _shouldWriteToLogServer = !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["domain"]);

            if (_shouldWriteToLogServer)
            {
                _url = $"{ConfigurationManager.AppSettings["domain"]}/logs";
                LoadCurrentIP();
            }
        }

        private void LoadCurrentIP()
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                var ips = host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
                _localIP = ips?.LastOrDefault()?.ToString();

            }
            catch (Exception)
            {

            }
        }

        public async Task Info(string msg)
        {
            _logger.Info(msg);
            if (_shouldWriteToLogServer)
            {
                await WriteToLogServerAsync(LogLevel.Information, msg);
            }
        }

        public async Task Debug(string msg)
        {
            _logger.Debug(msg);
            if (_shouldWriteToLogServer)
            {
                await WriteToLogServerAsync(LogLevel.Debug, msg);

            }
        }

        public async Task Error(string msg)
        {
            _logger.Error(msg);
            if (_shouldWriteToLogServer)
            {
                await WriteToLogServerAsync(LogLevel.Error, msg);
            }
        }


        private async Task WriteToLogServerAsync(LogLevel logLevel, string msg)
        {
            try
            {
                var request = new LogMessageDTO
                {
                    LogLevel = logLevel,
                    Message = msg,
                    ApplicationName = "Smart Card Desktop Client",
                    TimeStamp = DateTime.UtcNow,
                    ClientIP = _localIP
                };
                using (HttpClient client = new HttpClient())
                {
                    string content = JsonConvert.SerializeObject(request).ToString();
                    StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                    HttpResponseMessage serverResponse = await client.PostAsync(_url, stringContent);
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to write to log server", ex);
            }
        }
    }

    public class LogMessageDTO
    {
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public string ApplicationName { get; set; }
        public string Exception { get; set; }
        public string ClientIP { get; set; }
    }

    public enum LogLevel
    {
        All = 0,
        Debug = 1,
        Information = 2,
        Error = 3
    }

}
