using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SmartCardSigningFlow
    {
        public Guid Token { get; set; }
        public List<SmartCardSignFlowFields> Fields { get; set; } = new List<SmartCardSignFlowFields>();
    }
}
