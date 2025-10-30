using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WSE_ADAuth.Models
{
    public class AutoUserCreatingSettings
    {
       
        public string ManagementAPIURL { get; set; }
        public string PlanID { get; set; }
        public int ValidTillInMonths { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public bool Active { get;  set; }
    }
}
