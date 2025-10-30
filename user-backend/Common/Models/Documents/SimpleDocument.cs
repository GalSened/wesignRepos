using Common.Enums.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Documents
{
    public class SimpleDocument
    {
        public string RediretUrl { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public Template Template { get; set; }
        public Contact Contact { get; set; }
    }
}
