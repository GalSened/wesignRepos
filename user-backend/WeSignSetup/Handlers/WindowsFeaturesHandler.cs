/*
 * https://docs.microsoft.com/en-us/windows/win32/wmisdk/win32-serverfeature
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace WeSignSetup.Handlers
{
    public class WindowsFeaturesHandler
    {
        private readonly LogHandler _logHandler;

        public WindowsFeaturesHandler(LogHandler logHandler)
        {
            _logHandler = logHandler;
        }


        public void Check()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "InstallWindowsFetures - Copy.ps1");
            string powerShell = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe";
            _logHandler.Debug($"script path = [{filePath}]", false);
            if (File.Exists(filePath))
            {
                //File.GetAttributes(filePath);
                string strCmdText = filePath;
                using (var process = new Process())
                {

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                   //process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = powerShell;
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.Arguments = "\"&'" + strCmdText + "'\"";

                    process.Start();
                    string s = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();


                    using (StreamWriter outfile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "StandardOutput.txt"), true))
                    {

                        outfile.Write(s);
                    }
                }


            }
            //string resourceFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources");
            //string script = Directory.GetFiles(resourceFolder).FirstOrDefault(x => x.ToLower().EndsWith("ps1"));
            //string powershellExe = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe";
            //if (File.Exists(script) && File.Exists(powershellExe))
            //{
            //    using (var process = Process.Start(new ProcessStartInfo
            //    {
            //        FileName = powershellExe,
            //        Arguments = "\"&'" + script + "'\"",
            //        CreateNoWindow = true,
            //        WindowStyle = ProcessWindowStyle.Hidden,
            //        RedirectStandardOutput = true,
            //        UseShellExecute = false,
            //        Verb = "runas"
            //    }))
            //    {
            //        string s = process.StandardOutput.ReadToEnd();
            //        process.WaitForExit();
            //        _logHandler.Debug(s, false);
            //    }
            //}
        }
    }
}
