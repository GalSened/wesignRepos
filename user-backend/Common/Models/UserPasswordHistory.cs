using System;

namespace Common.Models
{
    public class UserPasswordHistory
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Password { get; set; }
        public DateTime CreationTime { get; set; }
        public UserPasswordHistory()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
}
