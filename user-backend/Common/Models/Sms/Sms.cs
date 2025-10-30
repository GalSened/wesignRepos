
namespace Common.Models.Sms
{
    using System.Collections.Generic;
    public class Sms
    {
        public IEnumerable<string> Phones { get; set; }
        public string Message { get; set; }
    }
}
