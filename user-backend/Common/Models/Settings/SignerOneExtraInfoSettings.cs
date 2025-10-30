using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Settings
{
    public class SignerOneExtraInfoSettings
    {
        public string CertPath { get; set; }
        public string Key1 { get; set; }
        public string RestUrl { get; set; } = "json/";
        public string PersistentKey2 { get; set; }
        public bool CertPinLogic { get; set; }
        public bool UseAuthAssertion { get; set; }

        public List<ExtraInfoHeaders> Headers { get; set;} = new List<ExtraInfoHeaders>();
    }

    public class ExtraInfoHeaders
    {
        public string Name { get; set;}
        public string Value { get; set; }
    }
}
