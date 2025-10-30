using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartCardDesktopClient
{
    static class Program
    {
        private static readonly Logger _logger = new Logger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>
        /// //Angular client will call using browser to :smartCardDesktopClient:/roomId_host
        //For example : smartCardDesktopClient:/123_http://njndjndj
        /// </remarks>
        [STAThread]
        static async Task Main(string[] args)
        {
            try
            {
                _logger.Info($"Main - Open Smart Card Desktop application").GetAwaiter().GetResult();
                //prevent launching application multiple times
                string thisprocessname = Process.GetCurrentProcess().ProcessName;
                var others = Process.GetProcesses().Where(x => x.ProcessName == thisprocessname && x.Id != Process.GetCurrentProcess().Id); ;
                foreach (var item in others)
                {
                    item.Kill();
                }
                _logger.Debug($"Main - Process name = {thisprocessname} , instance of process count = {Process.GetProcesses().Count(p => p.ProcessName == thisprocessname)}").GetAwaiter().GetResult();
                if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
                {
                    _logger.Error("Main - Application with same name already running...").GetAwaiter().GetResult();
                    return;
                }

                _logger.Debug($"Main - Start - app args : {string.Join(";", args)}").GetAwaiter().GetResult();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                (string RoomId, string Host) = ExtractDataFromArgs(args);
               // Debugger.Launch();
                var smartCardHandler = new SmartCardHandler(RoomId, Host);
                
                smartCardHandler.ShowCertificateSelctionUI();
                if (!smartCardHandler.AppClosed)
                {
                    Application.Run(smartCardHandler);
                    
                }

                _logger.Info($"Main - End Smart Card Desktop application").GetAwaiter().GetResult();
                smartCardHandler.Dispose();
                thisprocessname = Process.GetCurrentProcess().ProcessName;
                others = Process.GetProcesses().Where(x => x.ProcessName == thisprocessname ); 
                foreach (var item in others)
                {
                    item.Kill();
                }
                _logger.Info($"Main - End Kill All Process").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error($"Main - Application failed : {ex}").GetAwaiter().GetResult();
            }
        }
    

        private static (string RoomId, string Host) ExtractDataFromArgs(string[] args)
        {
            _logger.Debug($"ExtractDataFromArgs - {string.Join(";", args)}").GetAwaiter().GetResult();

            if (args.Length != 1)
            {
                _logger.Debug("ExtractDataFromArgs - App expected to get 1 parameter, which is : roomId and host").GetAwaiter().GetResult();
                Application.Exit();
            }
            if (!args[0].Contains('_'))
            {
                _logger.Error("ExtractDataFromArgs - application parameter should contain '_' as separator of 2 parameters").GetAwaiter().GetResult();
                Application.Exit();
            }

            var array = args[0].ToLower().Replace("smartcarddesktopclient:/", "").Split('_');
            string roomId = array.FirstOrDefault();
            string host = array.Length == 2 ? array.LastOrDefault() : array.Length == 3 ? $"{array[1]}_{array[2]}" : array.LastOrDefault();

            _logger.Debug($"ExtractDataFromArgs - Extract Data Success: roomId = {roomId}, host = {host}").GetAwaiter().GetResult();

            return (roomId, host);
        }
    }
}
