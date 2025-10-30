using System;

namespace Common.Models.Users
{
    public class CaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public DateTime Challenge_ts { get; set; }
        public string Hostname { get; set; }
    }
}
