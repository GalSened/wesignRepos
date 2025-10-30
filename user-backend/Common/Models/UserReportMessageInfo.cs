using Common.Models.Users;

namespace Common.Models
{
    public class UserReportMessageInfo : MessageInfo
    {
        public UserPeriodicReport Report { get; set; }
    }
}
