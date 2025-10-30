using System;
using Microsoft.Win32;

namespace CustomUrlProtocolInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            RegisterURLProtocol("smartCardDesktopClient", @"C:\repos\.net-framework\wesign3-backend\SmartCardDesktopClient\bin\Debug\netcoreapp3.1\SmartCardDesktopClient.exe");
            Console.WriteLine($"Successfully create registry");
            Console.ReadKey();
        }

        /// </summary>
        /// <param name="protocolName">Name of the protocol (e.g. "technothirsty"")</param>
        /// <param name="applicationPath">Complete file system path to the EXE file, which processes the URL being called.</param>
        /// Example to run exe : smartCardDesktopClient:/123 will open smartCardDesktopClient.exe with pararmeter smartCardDesktopClient:/123
        public static void RegisterURLProtocol(string protocolName, string applicationPath)
        {
            try
            {
                // Create new key for desired URL protocol
                var KeyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
                RegistryKey key = KeyTest.CreateSubKey(protocolName);
                key.SetValue("URL Protocol", protocolName);
                //key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + applicationPath + "\"");
                key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + applicationPath + "\" \"%1\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
        }
    }
}
