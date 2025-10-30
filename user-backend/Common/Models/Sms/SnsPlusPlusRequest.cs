using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Sms
{
    public class SnsPlusPlusRequest
    {
        public string Uid { get; set; }
        public string Pass { get; set; }
       // public string Initiator { get; set; }
        public string Sender { get; set; }
        public int Pri { get; set; }
        public string Msg { get; set; }
        public string To { get; set; }

    }
}

