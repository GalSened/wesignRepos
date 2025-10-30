using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WeSignSetup.Models;

namespace WeSignSetup.Handlers
{
    public class SilentInstallationHandler
    {
        private readonly LogHandler _logHandler;
        

        public SilentInstallationHandler(LogHandler logHandler)
        {
            _logHandler = logHandler;        
        }

        public void InstallExeFiles()
        {
            _logHandler.Debug("SilentInstallationHandler - start install all prerequisite application .");
       

            DotNetCoreInstallation();
            DotNetFramworkInstallation();
            LibreOfficeInstallation();
            UrlRewriteInstallation();
        }

        #region Private Functions

        private void DotNetCoreInstallation()
        {
            try
            {
                string dotNetCoreExe = Path.Combine(Path.GetDirectoryName(Folders.BaseFolder), "Sites", "dotnet-hosting-6.0.19-win.exe");
                _logHandler.Debug($"SilentInstallationHandler - dotNetCoreExe file [{dotNetCoreExe}].");
                string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _logHandler.Debug($"SilentInstallationHandler - currentFolder [{currentFolder}].");
                string command = $"{dotNetCoreExe} /install /norestart /quiet /log '{currentFolder}\\Microsoft.NET Core SDK 6.0.19.log'";
                if (!File.Exists(dotNetCoreExe))
                {
                    throw new Exception("Dot Net Core installation file not found");
                }
                RunCmd(command);
            }
            catch (Exception ex)
            {
                _logHandler.Error("", ex);
            }
        }

        private void DotNetFramworkInstallation()
        {
            try
            {
                string dotNetFramworkExe = Path.Combine(Path.GetDirectoryName(Folders.BaseFolder), "Sites", "NDP472-KB4054530-x86-x64-AllOS-ENU.exe"); 
                _logHandler.Debug($"SilentInstallationHandler - dotNetFramworkExe [{dotNetFramworkExe}].");
                string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _logHandler.Debug($"SilentInstallationHandler - currentFolder [{currentFolder}].");
                string command = $"{dotNetFramworkExe} /q /norestart /log '{currentFolder}\\Microsoft.NET Framework 4.7.2.log'";

                if (!File.Exists(dotNetFramworkExe))
                {
                    throw new Exception("Dot Net Framework installation file not found");
                }
                RunCmd(command);
            }
            catch (Exception ex)
            {
                _logHandler.Error("", ex);
            }
        }

        private void LibreOfficeInstallation()
        {
            try
            {
                string libreMsi = Path.Combine(Path.GetDirectoryName(Folders.BaseFolder), "Sites", "LibreOffice_6.3.5_Win_x64.msi");
                _logHandler.Debug($"SilentInstallationHandler - libreMsi file [{libreMsi}].");
                string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _logHandler.Debug($"SilentInstallationHandler - currentFolder [{currentFolder}].");
                string command = $"start /wait Msiexec /i '{libreMsi}' /qn /norestart ALLUSERS=1 CREATEDESKTOPLINK=0 REGISTER_ALL_MSO_TYPES=0 REGISTER_NO_MSO_TYPES=1 ISCHECKFORPRODUCTUPDATES=0 QUICKSTART=1 ADDLOCAL=ALL UI_LANGS=en_US,fr,es /log '{currentFolder}\\libreoffice.log' ";

                if (!File.Exists(libreMsi))
                {
                    throw new Exception("LibreOffice installation file not found");
                }
                RunCmd(command);
            }
            catch (Exception ex)
            {
                _logHandler.Error("", ex);
            }
        }

        private void UrlRewriteInstallation()
        {
            try
            {
                string urlrewrite2Exe = Path.Combine(Path.GetDirectoryName(Folders.BaseFolder), "Sites", "urlrewrite2.exe");
                _logHandler.Debug($"SilentInstallationHandler - urlrewrite2Exe [{urlrewrite2Exe}].");
                string currentFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _logHandler.Debug($"SilentInstallationHandler - currentFolder [{currentFolder}].");
                string command = $"{urlrewrite2Exe} /qn /norestart /log '{currentFolder}\\urlRewrite.log'";

                if (!File.Exists(urlrewrite2Exe))
                {
                    throw new Exception("urlRewrite installation file not found");
                }
                RunCmd(command);
            }
            catch (Exception ex)
            {
                _logHandler.Error("", ex);
            }
        }

        private void RunCmd(string command)
        {
            _logHandler.Debug($"SilentInstallationHandler - command to run [{command}].");

            using (Process process = new Process())
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}"
                };
                process.StartInfo = startInfo;
                process.Start();
                _logHandler.Debug($"SilentInstallationHandler - start running command [{command}] using CMD.EXE ");

            }
        }

        #endregion
    }
}
