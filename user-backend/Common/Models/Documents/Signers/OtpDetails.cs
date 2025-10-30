using Common.Enums;
using System;

namespace Common.Models.Documents.Signers
{
    public class OtpDetails
    {
        public string Identification { get; set; }
        public string Code { get; set; }
        public int Attempts { get; set; }
        public DateTime ExpirationTime { get; set; }
        public OtpMode Mode { get; set; }
    }
}