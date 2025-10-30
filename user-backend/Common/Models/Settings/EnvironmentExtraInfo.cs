using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Settings
{
    public class EnvironmentExtraInfo
    {
        public bool MamazFlow { get; set; }
        public string HostedAppHeaderKey { get; set; }

        public List<ExtraInfoHeaders> NotifyHeaders { get; set; } = new List<ExtraInfoHeaders>();

    }
    
}
