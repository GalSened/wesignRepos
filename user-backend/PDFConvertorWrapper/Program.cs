using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace PDFConvertorWrapper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 5)
            {
                var configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var configSection = configBuilder.GetSection("AppSettings");
                var libretool = configSection["LibreOfficePath"]?.ToString() ?? null;
                if (libretool != null)
                {
                    using (Process process = Process.Start(libretool, $"{UsingLoopStringBuilder(args)}"))
                    {
                        int count = 3;
                        while (!process.HasExited && count > 0)
                        {
                            process.WaitForExit(1000 * 5);
                            count--;
                        }

                        Process[] ps = Process.GetProcessesByName("soffice.bin");
                        foreach (var p in ps)
                        {
                            p.Kill();

                        }

                    }
                }
            }

        }

        public static string UsingLoopStringBuilder(string[] array)
        {
            var sb = new StringBuilder();           
            foreach (var item in array)
            {
                sb.Append($"{item} ");
                
            }
            return sb.ToString();
        }
    }
}