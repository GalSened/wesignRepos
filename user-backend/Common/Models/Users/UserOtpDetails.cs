using Common.Enums.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Users
{
    public class UserOtpDetails
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public UserOtpMode OtpMode { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
